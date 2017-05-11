using Laser.Orchard.PaymentGateway;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Orchard.Data;
using Laser.Orchard.NwazetIntegration.Models;
using AutoMapper;
using System;
using Orchard;
using Laser.Orchard.CommunicationGateway;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.ContentManagement;
using Laser.Orchard.CommunicationGateway.Models;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class PaymentEventHandler : IPaymentEventHandler {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IShoppingCart _shoppingCart;
        private readonly IPosServiceIntegration _posServiceIntegration;
        private readonly IRepository<AddressRecord> _addressRecord;
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;


        public PaymentEventHandler(IOrderService orderService, IPaymentService paymentService, IShoppingCart shoppingCart, IPosServiceIntegration posServiceIntegration, IRepository<AddressRecord> addressRecord, IOrchardServices orchardServices, ICommunicationService communicationService) {
            _orderService = orderService;
            _paymentService = paymentService;
            _shoppingCart = shoppingCart;
            _posServiceIntegration = posServiceIntegration;
            _addressRecord = addressRecord;
            _orchardServices = orchardServices;
            _communicationService = communicationService;
        }
        public void OnError(int paymentId, int contentItemId) {
            var payment = _paymentService.GetPayment(paymentId);
            if (payment != null) {
                var order = _orderService.Get(payment.ContentItemId);
                order.Status = OrderPart.Cancelled;
                order.LogActivity(OrderPart.Error, string.Format("Transaction failed (payment id: {0}).", payment.Id));
            }
        }

        public void OnSuccess(int paymentId, int contentItemId) {
            var payment = _paymentService.GetPayment(paymentId);
            if (payment != null) {
                var order = _orderService.Get(payment.ContentItemId);
                // agggiorna l'odine in base al pagamento effettuato
                order.Status = OrderPart.Pending;
                order.AmountPaid = (double)payment.Amount;
                order.PurchaseOrder = _posServiceIntegration.GetOrderNumber(order.Id);
                order.CurrencyCode = payment.Currency;
                order.LogActivity(OrderPart.Event, string.Format("Payed on POS {0}.", payment.PosName));
                // svuota il carrello
                _shoppingCart.Clear();
                OrderToContact(order);

            }
        }

        private void OrderToContact(OrderPart order) {
            // recupero il contatto
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            List<ContentItem> ContactList = new List<ContentItem>();
            if (currentUser != null) {
                var contactpart = _communicationService.GetContactFromUser(currentUser.Id);
                if (contactpart == null) { // non dovrebbe mai succedere (inserito nel caso cambiassimo la logica già implementata)
                    _communicationService.UserToContact(currentUser);
                    contactpart = _communicationService.GetContactFromUser(currentUser.Id);
                }
                ContactList.Add(contactpart.ContentItem);
            }
            else {
                var contacts = _communicationService.GetContactsFromMail(order.CustomerEmail);
                if (contacts.Count > 0) {
                    ContactList = contacts;
                }
                else {
                    var newcontact = _orchardServices.ContentManager.Create("CommunicationContact", VersionOptions.Draft);
                    ((dynamic)newcontact).CommunicationContactPart.Master = false;
                    ContactList.Add(newcontact);
                }
            }
            foreach (var contactItem in ContactList) {
                StoreAddress(order.BillingAddress, "BillingAddress", contactItem);
                StoreAddress(order.ShippingAddress, "ShippingAddress", contactItem);
                _communicationService.AddEmailToContact(order.CustomerEmail, contactItem);
                _communicationService.AddSmsToContact("0039", order.CustomerPhone, contactItem);
            }
        }

        private void StoreAddress(Address address, string typeAddress, ContentItem contact) {
            var typeAddressValue = (AddressRecordType)Enum.Parse(typeof(AddressRecordType), typeAddress);
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Address, AddressRecord>();
            });
            var addressToStore = new AddressRecord();
            Mapper.Map<Address, AddressRecord>(address, addressToStore);
            addressToStore.AddressType = typeAddressValue;
            addressToStore.NwazetAddressRecord_Id = contact.Id;
            bool AddNewAddress = true;
            foreach (var existingAddressRecord in contact.As<NwazetContactPart>().NwazetAddressRecord) {
                if (addressToStore.Equals(existingAddressRecord)) {
                    AddNewAddress = false;
                    existingAddressRecord.TimeStampUTC = DateTime.UtcNow;
                    _addressRecord.Update(existingAddressRecord);
                    _addressRecord.Flush();
                }
                }
            if (AddNewAddress) {
                _addressRecord.Create(addressToStore);
                _addressRecord.Flush();
            }
        }
    }
}