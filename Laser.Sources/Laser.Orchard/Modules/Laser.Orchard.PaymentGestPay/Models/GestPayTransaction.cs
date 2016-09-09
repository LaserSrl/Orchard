using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Models {
    public class GestPayTransaction {
        #region Mandatory properties
        [StringLength(3)]
        public string uicCode { get; set; } //code identifying currency in which transaction amount is denominated
        [StringLength(9)]
        public string amount { get; set; } //Transaction amount. Do not insert thousands separator. Decimals, max 2 digits, are optional and separator is point
        [StringLength(50)]
        public string shopTransactionID { get; set; } //Identifier attributed to merchant's transaction
        #endregion
        #region Optional properties
        public string cardNumber { get; set; }
        public string expiryMonth { get; set; }
        public string expiryYear { get; set; }
        [StringLength(50)]
        public string buyerName { get; set; } //Buyer's name and surname
        [StringLength(50)]
        public string buyerEmail { get; set; } //Buyer's email address
        [StringLength(2)]
        public string languageId { get; set; } //Code identifying language used in communication with buyer
        public string cvv { get; set; }
        [StringLength(1000)]
        public string customInfo { get; set; } //String containing specific infomation as configured in the merchant's profile
        [StringLength(25)]
        public string requestToken { get; set; } //"MASKEDPAN" for a standard token, any other value for Custom token. Using :FORCED: before the token, it's possible to have the token even if the transaction is not authorized
        [StringLength(1)]
        public string ppSellerProtection { get; set; } //Parameter to set the use of a confirmed address
        public GenericShippingDetails shippingDetails { get; set; }
        public string[] paymentTypes { get; set; } //set of tags to set the visibility of payment systems on payment page (see CodeTables.PaymentTypeCodes)
        public GenericPaymentTypeDetail paymentTypeDetail { get; set; }
        [StringLength(1)]
        public string redFraudPrevention { get; set; } //flag to activate Red Fraud Prevention (redFraudPrevention = "1")
        public GenericRedCustomerInfo Red_CustomerInfo { get; set; }
        public GenericRedShippingInfo Red_ShippingInfo { get; set; }
        public GenericRedBillingInfo Red_BillingInfo { get; set; }
        public GenericRedCustomerData Red_CustomerData { get; set; }
        public string[] Red_CustomInfo { get; set; }
        public GenericRedItems Red_Items { get; set; }
        [StringLength(3)]
        public string Consel_MerchantPro { get; set; } //merchant promotional code (mandatory to show consel in the pagam's payment method)
        public GenericConselCustomerInfo Consel_CustomerInfo { get; set; }
        [StringLength(127)]
        public string payPalBillingAgreementDescription { get; set; } //description of the goods, terms and conditions of the billing agreement
        public GenericEcommGestpayPaymentDetails OrderDetails { get; set; }
        #endregion
    }
    /// <summary>
    /// This class contains the same exact information of the ShippingDetails classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericShippingDetails {
        [StringLength(32)]
        public string shipToName { get; set; } //string containing the shipping name
        [StringLength(100)]
        public string shipToStreet { get; set; } //string containing the shipping address
        [StringLength(40)]
        public string shipToCity { get; set; } //string containing the shipping city
        [StringLength(40)]
        public string shipToState { get; set; } //string containing the shipping state (see CodeTables.StateCodes)
        [StringLength(2)]
        public string shipToCountryCode { get; set; } //string containing the shipping country code (see CodeTables.ISOCountryCodes)
        [StringLength(20)]
        public string shipToZip { get; set; } //string containing the shipping zip
        [StringLength(100)]
        public string shipToStreet2 { get; set; } //string containing a shipping address additional field

        //The following methods are used to get the correct object according to the environment being used
        /// <summary>
        /// This method computes the object used to provide shipping details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ShippingDetails TestVersion() {
            return new CryptDecryptTest.ShippingDetails {
                shipToName = this.shipToName,
                shipToStreet = this.shipToStreet,
                shipToCity = this.shipToCity,
                shipToState = this.shipToState,
                shipToCountryCode = this.shipToCountryCode,
                shipToZip = this.shipToZip,
                shipToStreet2 = this.shipToStreet2
            };
        }
        /// <summary>
        /// This method computes the object used to provide shipping details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ShippingDetails ProdVersion() {
            return new CryptDecryptProd.ShippingDetails {
                shipToName = this.shipToName,
                shipToStreet = this.shipToStreet,
                shipToCity = this.shipToCity,
                shipToState = this.shipToState,
                shipToCountryCode = this.shipToCountryCode,
                shipToZip = this.shipToZip,
                shipToStreet2 = this.shipToStreet2
            };
        }

    }
    /// <summary>
    /// This class contains the same exact information of the PaymentTypeDetail classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericPaymentTypeDetail {
        [StringLength(25)]
        public string MyBankBankCode { get; set; } //tag to set the Bank to show on payment page (the bank list is retrieved form WsS2S.CallMyBankListS2S)
        [StringLength(25)]
        public string IdealBankCode { get; set; } //tag to set the Bank to show on payment page (the bank list is retrieved form WsS2S.CallMyBankListS2S)

        /// <summary>
        /// This method computes the object used to provide payment type details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.PaymentTypeDetail TestVersion() {
            return new CryptDecryptTest.PaymentTypeDetail {
                MyBankBankCode = this.MyBankBankCode,
                IdealBankCode = this.IdealBankCode
            };
        }
        /// <summary>
        /// This method computes the object used to provide payment type details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.PaymentTypeDetail ProdVersion() {
            return new CryptDecryptProd.PaymentTypeDetail {
                MyBankBankCode = this.MyBankBankCode,
                IdealBankCode = this.IdealBankCode
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedCustomerInfo classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedCustomerInfo {
        [StringLength(5)]
        public string Customer_Title { get; set; } //customer title
        [StringLength(30)]
        public string Customer_Name { get; set; } //customer first name
        [StringLength(30)]
        public string Customer_Surname { get; set; } //customer last name
        [StringLength(45)]
        private string _customerEmail;
        public string Customer_Email {
            get { return _customerEmail; }
            set {
                if (value.Contains("@")) { //not a full regex validation of an email field
                    _customerEmail = value;
                } else {
                    throw new System.FormatException("Not a valid email address. Must contain @ character.");
                }
            }
        } //customer email address - value must contain @
        [StringLength(30)]
        public string Customer_Address { get; set; } //customer address line 1
        [StringLength(30)]
        public string Customer_Address2 { get; set; } //customer address line 2
        [StringLength(20)]
        public string Customer_City { get; set; } //customer address city
        [StringLength(2)]
        public string Customer_StateCode { get; set; } //customer address state code
        [StringLength(3)]
        public string Customer_Country { get; set; } //customer country code - ISO-Alpha 3 (see CodeTables.ISOCountryCodes)
        [StringLength(9)]
        public string Customer_PostalCode { get; set; } //customer post/zip code
        [StringLength(19)]
        private string _customerPhone;
        public string Customer_Phone {
            get { return _customerPhone; }
            set {
                string tmp = Regex.Replace(value, "[\n\r\t/s]", " ");
                if (tmp.Contains(" ")) {
                    throw new System.FormatException("Not a valid phone. Must not contain whitespace.");
                } else {
                    _customerPhone = value;
                }
            }
        } //Customer phone - no spaces

        /// <summary>
        /// This method computes the object used to provide customer info details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedCustomerInfo TestVersion(){
            return new CryptDecryptTest.RedCustomerInfo {
                Customer_Title = this.Customer_Title,
                Customer_Name = this.Customer_Name,
                Customer_Surname = this.Customer_Surname,
                Customer_Email = this.Customer_Email,
                Customer_Address = this.Customer_Address,
                Customer_Address2 = this.Customer_Address2,
                Customer_City = this.Customer_City,
                Customer_StateCode = this.Customer_StateCode,
                Customer_Country = this.Customer_Country,
                Customer_PostalCode = this.Customer_PostalCode,
                Customer_Phone = this.Customer_Phone
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer info details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedCustomerInfo ProdVersion(){
            return new CryptDecryptProd.RedCustomerInfo {
                Customer_Title = this.Customer_Title,
                Customer_Name = this.Customer_Name,
                Customer_Surname = this.Customer_Surname,
                Customer_Email = this.Customer_Email,
                Customer_Address = this.Customer_Address,
                Customer_Address2 = this.Customer_Address2,
                Customer_City = this.Customer_City,
                Customer_StateCode = this.Customer_StateCode,
                Customer_Country = this.Customer_Country,
                Customer_PostalCode = this.Customer_PostalCode,
                Customer_Phone = this.Customer_Phone
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedShippingInfo classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedShippingInfo {
        [StringLength(30)]
        public string Shipping_Name { get; set; } //shipping first name
        [StringLength(30)]
        public string Shipping_Surname { get; set; } //shipping last name
        [StringLength(45)]
        private string _shippingEmail;
        public string Shipping_Email {
            get{return _shippingEmail;}
            set {
                if (value.Contains("@")) { //not a full regex validation of an email field
                    _shippingEmail = value;
                } else {
                    throw new System.FormatException("Not a valid email address. Must contain @ character.");
                }
            }
        } //customer email address - value must contain @
        [StringLength(30)]
        public string Shipping_Address { get; set; } //shipping address line 1
        [StringLength(30)]
        public string Shipping_Address2 { get; set; } //shipping address line 2
        [StringLength(20)]
        public string Shipping_City { get; set; } //shipping address city
        [StringLength(2)]
        public string Shipping_StateCode { get; set; } //shipping address state code
        [StringLength(3)]
        public string Shipping_Country { get; set; } //shipping country code  - ISO-Alpha 3 (see CodeTables.ISOCountryCodes)
        [StringLength(9)]
        public string Shipping_PostalCode { get; set; } //shipping post/zip code
        [StringLength(19)]
        private string _homePhone;
        public string Shipping_HomePhone {
            get { return _homePhone; }
            set {
                string tmp = Regex.Replace(value, "[\n\r\t/s]", " ");
                if (tmp.Contains(" ")) {
                    throw new System.FormatException("Not a valid phone. Must not contain whitespace.");
                } else {
                    _homePhone = value;
                }
            }
        } //Customer home phone - no spaces
        [StringLength(12)]
        private string _faxPhone;
        public string Shipping_FaxPhone {
            get { return _faxPhone; }
            set {
                string tmp = Regex.Replace(value, "[\n\r\t/s]", " ");
                if (tmp.Contains(" ")) {
                    throw new System.FormatException("Not a valid phone. Must not contain whitespace.");
                } else {
                    _faxPhone = value;
                }
            }
        } //Customer fax phone - no spaces
        [StringLength(19)]
        public string Shipping_MobilePhone { get; set; } //Customer mobile phone
        [StringLength(19)]
        public string Shipping_TimeToDeparture { get; set; } //shipping time to departure

        /// <summary>
        /// This method computes the object used to provide customer shipping details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedShippingInfo TestVersion() {
            return new CryptDecryptTest.RedShippingInfo {
                Shipping_Name = this.Shipping_Name,
                Shipping_Surname = this.Shipping_Surname,
                Shipping_Email = this.Shipping_Email,
                Shipping_Address = this.Shipping_Address,
                Shipping_Address2 = this.Shipping_Address2,
                Shipping_City = this.Shipping_City,
                Shipping_StateCode = this.Shipping_StateCode,
                Shipping_Country = this.Shipping_Country,
                Shipping_PostalCode = this.Shipping_PostalCode,
                Shipping_HomePhone = this.Shipping_HomePhone,
                Shipping_FaxPhone = this.Shipping_FaxPhone,
                Shipping_MobilePhone = this.Shipping_MobilePhone,
                Shipping_TimeToDeparture = this.Shipping_TimeToDeparture
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer shipping details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedShippingInfo ProdVersion() {
            return new CryptDecryptProd.RedShippingInfo {
                Shipping_Name = this.Shipping_Name,
                Shipping_Surname = this.Shipping_Surname,
                Shipping_Email = this.Shipping_Email,
                Shipping_Address = this.Shipping_Address,
                Shipping_Address2 = this.Shipping_Address2,
                Shipping_City = this.Shipping_City,
                Shipping_StateCode = this.Shipping_StateCode,
                Shipping_Country = this.Shipping_Country,
                Shipping_PostalCode = this.Shipping_PostalCode,
                Shipping_HomePhone = this.Shipping_HomePhone,
                Shipping_FaxPhone = this.Shipping_FaxPhone,
                Shipping_MobilePhone = this.Shipping_MobilePhone,
                Shipping_TimeToDeparture = this.Shipping_TimeToDeparture
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedBillingInfo classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedBillingInfo {
        [StringLength(16)]
        public string Billing_Id { get; set; } //billing id
        [StringLength(30)]
        public string Billing_Name { get; set; } //Billing first name
        [StringLength(30)]
        public string Billing_Surname { get; set; } //Billing last name
        [StringLength(10)]
        public string Billing_DateOfBirth { get; set; } //billing date of birth - format YYYYMMDD
        [StringLength(45)]
        private string _billingEmail;
        public string Billing_Email {
            get { return _billingEmail; }
            set {
                if (value.Contains("@")) { //not a full regex validation of an email field
                    _billingEmail = value;
                } else {
                    throw new System.FormatException("Not a valid email address. Must contain @ character.");
                }
            }
        } //billing email address - value must contain @
        [StringLength(30)]
        public string Billing_Address { get; set; } //Billing address line 1
        [StringLength(30)]
        public string Billing_Address2 { get; set; } //Billing address line 2
        [StringLength(20)]
        public string Billing_City { get; set; } //Billing address city
        [StringLength(2)]
        public string Billing_StateCode { get; set; } //Billing address state code
        [StringLength(3)]
        public string Billing_Country { get; set; } //Billing country code  - ISO-Alpha 3 (see CodeTables.ISOCountryCodes)
        [StringLength(9)]
        public string Billing_PostalCode { get; set; } //Billing post/zip code
        [StringLength(19)]
        private string _homePhone;
        public string Billing_HomePhone {
            get { return _homePhone; }
            set {
                string tmp = Regex.Replace(value, "[\n\r\t/s]", " ");
                if (tmp.Contains(" ")) {
                    throw new System.FormatException("Not a valid phone. Must not contain whitespace.");
                } else {
                    _homePhone = value;
                }
            }
        } //billing home phone - no spaces
        [StringLength(19)]
        private string _workPhone;
        public string Billing_WorkPhone {
            get { return _workPhone; }
            set {
                string tmp = Regex.Replace(value, "[\n\r\t/s]", " ");
                if (tmp.Contains(" ")) {
                    throw new System.FormatException("Not a valid phone. Must not contain whitespace.");
                } else {
                    _workPhone = value;
                }
            }
        } //billing work phone - no spaces
        [StringLength(19)]
        public string Billing_MobilePhone { get; set; } //billing mobile phone

        /// <summary>
        /// This method computes the object used to provide customer billing details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedBillingInfo TestVersion() {
            return new CryptDecryptTest.RedBillingInfo {
                Billing_Id = this.Billing_Id,
                Billing_Name = this.Billing_Name,
                Billing_Surname = this.Billing_Surname,
                Billing_DateOfBirth = this.Billing_DateOfBirth,
                Billing_Email = this.Billing_Email,
                Billing_Address = this.Billing_Address,
                Billing_Address2 = this.Billing_Address2,
                Billing_City = this.Billing_City,
                Billing_StateCode = this.Billing_StateCode,
                Billing_Country = this.Billing_Country,
                Billing_PostalCode = this.Billing_PostalCode,
                Billing_HomePhone = this.Billing_HomePhone,
                Billing_WorkPhone = this.Billing_WorkPhone,
                Billing_MobilePhone = this.Billing_MobilePhone
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer billing details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedBillingInfo ProdVersion() {
            return new CryptDecryptProd.RedBillingInfo {
                Billing_Id = this.Billing_Id,
                Billing_Name = this.Billing_Name,
                Billing_Surname = this.Billing_Surname,
                Billing_DateOfBirth = this.Billing_DateOfBirth,
                Billing_Email = this.Billing_Email,
                Billing_Address = this.Billing_Address,
                Billing_Address2 = this.Billing_Address2,
                Billing_City = this.Billing_City,
                Billing_StateCode = this.Billing_StateCode,
                Billing_Country = this.Billing_Country,
                Billing_PostalCode = this.Billing_PostalCode,
                Billing_HomePhone = this.Billing_HomePhone,
                Billing_WorkPhone = this.Billing_WorkPhone,
                Billing_MobilePhone = this.Billing_MobilePhone
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedCustomerData classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedCustomerData {
        [StringLength(60)]
        public string MerchantWebSite { get; set; } //transaction source website
        [StringLength(45)]
        public string Customer_IpAddress { get; set; } //ip of customer - Format: nnn.nnn.nnn.nnn
        [StringLength(4000)]
        public string PC_FingerPrint { get; set; } //PC Finger Print. If the RED configuration is defined with the chance to fill this valu, but for some reasons it's left empty, then fill Red=ServiceType="N" to avoid error
        [StringLength(1)]
        public string PreviousCustomer { get; set; } //previous customer flag - format "Y" or "N"
        [StringLength(12)]
        public string Red_Merchant_ID { get; set; } //optional only for merchant with a specific set of rules (code provided by Sella)
        [StringLength(1)]
        public string Red_ServiceType { get; set; } //optional only for merchant with a specific set of rules (code provided by Sella)

        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedCustomerData TestVersion() {
            return new CryptDecryptTest.RedCustomerData {
                MerchantWebSite = this.MerchantWebSite,
                Customer_IPAddress = this.Customer_IpAddress,
                PC_FingerPrint = this.PC_FingerPrint,
                PreviousCustomer = this.PreviousCustomer,
                Red_Merchant_ID = this.Red_Merchant_ID,
                Red_ServiceType = this.Red_ServiceType
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedCustomerData ProdVersion() {
            return new CryptDecryptProd.RedCustomerData {
                MerchantWebSite = this.MerchantWebSite,
                Customer_IPAddress = this.Customer_IpAddress,
                PC_FingerPrint = this.PC_FingerPrint,
                PreviousCustomer = this.PreviousCustomer,
                Red_Merchant_ID = this.Red_Merchant_ID,
                Red_ServiceType = this.Red_ServiceType
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedItems classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedItems {
        [StringLength(2)]
        public string NumberOfItems { get; set; }
        public GenericRedItem[] Red_Item { get; set; }

        /// <summary>
        /// This method computes the object used to provide item details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedItems TestVersion() {
            return new CryptDecryptTest.RedItems {
                NumberOfItems = this.NumberOfItems,
                Red_Item = this.Red_Item.Select(ri => ri.TestVersion()).ToArray()
            };
        }
        /// <summary>
        /// This method computes the object used to provide item details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedItems ProdVersion() {
            return new CryptDecryptProd.RedItems {
                NumberOfItems = this.NumberOfItems,
                Red_Item = this.Red_Item.Select(ri => ri.ProdVersion()).ToArray()
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedItem classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedItem {
        [StringLength(12)]
        public string Item_ProductCode { get; set; } //item product code
        [StringLength(12)]
        public string Item_StockKeepingUnit { get; set; } //item stock keeping unit
        [StringLength(26)]
        public string Item_Description { get; set; } //item description
        [StringLength(12)]
        public string Item_Quantity { get; set; } //item quantity - 1 should be sent as "10000"
        [StringLength(12)]
        public string Item_UnitCost { get; set; } //item cost amount - €5.00 should be sent as 50000
        [StringLength(12)]
        public string Item_TotalCost { get; set; } //total item amount (item qty * item cost), no decimal
        [StringLength(19)]
        public string Item_ShippingNumber { get; set; } //item shippping/tracking numberr
        [StringLength(160)]
        public string Item_GiftMessage { get; set; } //item gift message
        [StringLength(30)]
        public string Item_PartEAN_Number { get; set; } //item Park or EAN number
        [StringLength(160)]
        public string Item_ShippingComments { get; set; } //item shipping comments

        /// <summary>
        /// This method computes the object used to provide item details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedItem TestVersion() {
            return new CryptDecryptTest.RedItem {
                Item_ProductCode = this.Item_ProductCode,
                Item_StockKeepingUnit = this.Item_StockKeepingUnit,
                Item_Description = this.Item_Description,
                Item_Quantity = this.Item_Quantity,
                Item_UnitCost = this.Item_UnitCost,
                Item_TotalCost = this.Item_TotalCost,
                Item_ShippingNumber = this.Item_ShippingNumber,
                Item_GiftMessage = this.Item_GiftMessage,
                Item_PartEAN_Number = this.Item_PartEAN_Number,
                Item_ShippingComments = this.Item_ShippingComments
            };
        }
        /// <summary>
        /// This method computes the object used to provide item details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedItem ProdVersion() {
            return new CryptDecryptProd.RedItem {
                Item_ProductCode = this.Item_ProductCode,
                Item_StockKeepingUnit = this.Item_StockKeepingUnit,
                Item_Description = this.Item_Description,
                Item_Quantity = this.Item_Quantity,
                Item_UnitCost = this.Item_UnitCost,
                Item_TotalCost = this.Item_TotalCost,
                Item_ShippingNumber = this.Item_ShippingNumber,
                Item_GiftMessage = this.Item_GiftMessage,
                Item_PartEAN_Number = this.Item_PartEAN_Number,
                Item_ShippingComments = this.Item_ShippingComments
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the ConselCustomerInfo classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericConselCustomerInfo {
        [StringLength(30)]
        public string Surname { get; set; } //customer surname
        [StringLength(30)]
        public string Name { get; set; } //customer name
        [StringLength(16)]
        public string TaxationCode { get; set; } //customer taxation code
        [StringLength(60)]
        public string Address { get; set; } //customer address
        [StringLength(30)]
        public string City { get; set; } //customer city
        [StringLength(2)]
        public string StateCode { get; set; } //customer state code
        [StringLength(10)]
        public string DateAddress { get; set; } //date since the customer lives in the declared address dd/mm/yyyy
        [StringLength(15)]
        public string Phone { get; set; } //customer phone
        [StringLength(15)]
        public string MobilePhone { get; set; } //customer mobile phone
        public string MunicipalCode { get; set; }
        public string StateBirthDate { get; set; }
        public string BirthDate { get; set; }
        public string Mail { get; set; }
        public string MunicipalDocumentCode { get; set; }
        public string Employment { get; set; }
        public string WorkingAddress { get; set; }
        public string MunicipalWorkingCode { get; set; }
        public string DocumentState { get; set; }
        public string DocumentNumber { get; set; }
        public string MunicipalBirthCode { get; set; }
        public string VisaExpiryDate { get; set; }
        public string Iban { get; set; }
        public string DocumentDate { get; set; }
        public string WorkingTelNumber { get; set; }
        public string WorkingState { get; set; }
        public string MonthlyPay { get; set; }

        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ConselCustomerInfo TestVersion() {
            return new CryptDecryptTest.ConselCustomerInfo {
                Surname = this.Surname,
                Name = this.Name,
                TaxationCode = this.TaxationCode,
                Address = this.Address,
                City = this.City,
                StateCode = this.StateCode,
                DateAddress = this.DateAddress,
                Phone = this.Phone,
                MobilePhone = this.MobilePhone,
                MunicipalCode = this.MunicipalCode,
                StateBirthDate = this.StateBirthDate,
                BirthDate = this.BirthDate,
                Mail = this.Mail,
                MunicipalDocumentCode = this.MunicipalDocumentCode,
                Employment = this.Employment,
                WorkingAddress = this.WorkingAddress,
                MunicipalWorkingCode = this.MunicipalWorkingCode,
                DocumentState = this.DocumentState,
                DocumentNumber = this.DocumentNumber,
                MunicipalBirthCode = this.MunicipalBirthCode,
                VisaExpiryDate = this.VisaExpiryDate,
                Iban = this.Iban,
                DocumentDate = this.DocumentDate,
                WorkingTelNumber = this.WorkingTelNumber,
                WorkingState = this.WorkingState,
                MonthlyPay = this.MonthlyPay
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ConselCustomerInfo ProdVersion() {
            return new CryptDecryptProd.ConselCustomerInfo {
                Surname = this.Surname,
                Name = this.Name,
                TaxationCode = this.TaxationCode,
                Address = this.Address,
                City = this.City,
                StateCode = this.StateCode,
                DateAddress = this.DateAddress,
                Phone = this.Phone,
                MobilePhone = this.MobilePhone,
                MunicipalCode = this.MunicipalCode,
                StateBirthDate = this.StateBirthDate,
                BirthDate = this.BirthDate,
                Mail = this.Mail,
                MunicipalDocumentCode = this.MunicipalDocumentCode,
                Employment = this.Employment,
                WorkingAddress = this.WorkingAddress,
                MunicipalWorkingCode = this.MunicipalWorkingCode,
                DocumentState = this.DocumentState,
                DocumentNumber = this.DocumentNumber,
                MunicipalBirthCode = this.MunicipalBirthCode,
                VisaExpiryDate = this.VisaExpiryDate,
                Iban = this.Iban,
                DocumentDate = this.DocumentDate,
                WorkingTelNumber = this.WorkingTelNumber,
                WorkingState = this.WorkingState,
                MonthlyPay = this.MonthlyPay
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the EcommGestpayPaymentDetails classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericEcommGestpayPaymentDetails {
        public GenericFraudPrevention FraudPrevention { get; set; }
        public GenericCustomerDetail CustomerDetail { get; set; }
        public GenericShippingAddress ShippingAddress { get; set; }
        public GenericBillingAddress BillingAddress { get; set; }
        public GenericProductDetail[] ProductDetails { get; set; }
        public GenericDiscountCode[] DiscountCodes { get; set; }
        public GenericShippingLine[] ShippingLines { get; set; }

        /// <summary>
        /// This method computes the object used to provide payment details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.EcommGestpayPaymentDetails TestVersion() {
            return new CryptDecryptTest.EcommGestpayPaymentDetails {
                FraudPrevention = this.FraudPrevention.TestVersion(),
                CustomerDetail = this.CustomerDetail.TestVersion(),
                ShippingAddress = this.ShippingAddress.TestVersion(),
                BillingAddress = this.BillingAddress.TestVersion(),
                ProductDetails = this.ProductDetails.Select(pd => pd.TestVersion()).ToArray(),
                DiscountCodes = this.DiscountCodes.Select(dc => dc.TestVersion()).ToArray(),
                ShippingLines = this.ShippingLines.Select(sl => sl.TestVersion()).ToArray()
            };
        }
        /// <summary>
        /// This method computes the object used to provide payment details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.EcommGestpayPaymentDetails ProdVersion() {
            return new CryptDecryptProd.EcommGestpayPaymentDetails {
                FraudPrevention = this.FraudPrevention.ProdVersion(),
                CustomerDetail = this.CustomerDetail.ProdVersion(),
                ShippingAddress = this.ShippingAddress.ProdVersion(),
                BillingAddress = this.BillingAddress.ProdVersion(),
                ProductDetails = this.ProductDetails.Select(pd => pd.ProdVersion()).ToArray(),
                DiscountCodes = this.DiscountCodes.Select(dc => dc.ProdVersion()).ToArray(),
                ShippingLines = this.ShippingLines.Select(sl => sl.ProdVersion()).ToArray()
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the FraudPrevention classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericFraudPrevention {
        public string SubmitForReview { get; set; }
        public string OrderDateTime { get; set; }
        public string OrderNote { get; set; }
        public string Source { get; set; }
        public string SubmissionReason { get; set; }
        public string BeaconSessionID { get; set; }

        /// <summary>
        /// This method computes the object used to provide fraud prevention details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.FraudPrevention TestVersion() {
            return new CryptDecryptTest.FraudPrevention {
                SubmitForReview = this.SubmitForReview,
                OrderDateTime = this.OrderDateTime,
                OrderNote = this.OrderNote,
                Source = this.Source,
                SubmissionReason = this.SubmissionReason,
                BeaconSessionID = this.BeaconSessionID
            };
        }
        /// <summary>
        /// This method computes the object used to provide fraud prevention details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.FraudPrevention ProdVersion() {
            return new CryptDecryptProd.FraudPrevention {
                SubmitForReview = this.SubmitForReview,
                OrderDateTime = this.OrderDateTime,
                OrderNote = this.OrderNote,
                Source = this.Source,
                SubmissionReason = this.SubmissionReason,
                BeaconSessionID = this.BeaconSessionID
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the CustomerDetail classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericCustomerDetail {
        [StringLength(12)]
        public string ProfileID { get; set; } //customer profile ID
        [StringLength(50)]
        public string MerchantCustomerID { get; set; } //merchant customer ID
        [StringLength(65)]
        public string FirstName { get; set; } //customer first name
        [StringLength(65)]
        public string MiddleName { get; set; } //customer middle name
        [StringLength(65)]
        public string Lastname { get; set; } //customer last name
        [StringLength(100)]
        public string PrimaryEmail { get; set; } //customer primary email
        [StringLength(100)]
        public string SecondaryEmail { get; set; } //customer secondary email
        [StringLength(20)]
        public string PrimaryPhone { get; set; } //customer's phone including prefix
        [StringLength(20)]
        public string SecondaryPhone { get; set; } //customer's phone including prefix
        [StringLength(10)]
        public string DateOfBirth { get; set; } //customer date of birth dd/mm/yyyy
        [StringLength(1)]
        public string Gender { get; set; } //customer gender ("0"=Male "1"=Female)
        [StringLength(20)]
        public string SocialSecurityNumber { get; set; } //customer's social or fiscal identifier (for klarna use)
        [StringLength(255)]
        public string Company { get; set; } //customer company
        public string CreatedAtDate { get; set; }
        public string VerfiedEmail { get; set; }
        public string AccountType { get; set; }
        public GenericCustomerSocial Social { get; set; }

        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.CustomerDetail TestVersion() {
            return new CryptDecryptTest.CustomerDetail {
                ProfileID = this.ProfileID,
                MerchantCustomerID = this.MerchantCustomerID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                PrimaryEmail = this.PrimaryEmail,
                SecondaryEmail = this.SecondaryEmail,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                DateOfBirth = this.DateOfBirth,
                Gender = this.Gender,
                SocialSecurityNumber = this.SocialSecurityNumber,
                Company = this.Company,
                CreatedAtDate = this.CreatedAtDate,
                VerifiedEmail = this.VerfiedEmail,
                AccountType = this.AccountType,
                Social = this.Social.TestVersion()
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.CustomerDetail ProdVersion() {
            return new CryptDecryptProd.CustomerDetail {
                ProfileID = this.ProfileID,
                MerchantCustomerID = this.MerchantCustomerID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                PrimaryEmail = this.PrimaryEmail,
                SecondaryEmail = this.SecondaryEmail,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                DateOfBirth = this.DateOfBirth,
                Gender = this.Gender,
                SocialSecurityNumber = this.SocialSecurityNumber,
                Company = this.Company,
                CreatedAtDate = this.CreatedAtDate,
                VerifiedEmail = this.VerfiedEmail,
                AccountType = this.AccountType,
                Social = this.Social.ProdVersion()
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the CustomerSocial classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericCustomerSocial {
        public string Network { get; set; }
        public string PublicUsername { get; set; }
        public string CommunityScore { get; set; }
        public string ProfilePicrture { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string AccountUrl { get; set; }
        public string Following { get; set; }
        public string Followed { get; set; }
        public string Posts { get; set; }
        public string Id { get; set; }
        public string AuthToken { get; set; }
        public string SocialData { get; set; }

        /// <summary>
        /// This method computes the object used to provide social data details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.CustomerSocial TestVersion() {
            return new CryptDecryptTest.CustomerSocial {
                Network = this.Network,
                PublicUsername = this.PublicUsername,
                CommunityScore = this.CommunityScore,
                ProfilePicture = this.ProfilePicrture,
                Email = this.Email,
                Bio = this.Bio,
                AccountUrl = this.AccountUrl,
                Following = this.Following,
                Followed = this.Followed,
                Posts = this.Posts,
                Id = this.Id,
                AuthToken = this.AuthToken,
                SocialData = this.SocialData
            };
        }
        /// <summary>
        /// This method computes the object used to provide social data details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.CustomerSocial ProdVersion() {
            return new CryptDecryptProd.CustomerSocial {
                Network = this.Network,
                PublicUsername = this.PublicUsername,
                CommunityScore = this.CommunityScore,
                ProfilePicture = this.ProfilePicrture,
                Email = this.Email,
                Bio = this.Bio,
                AccountUrl = this.AccountUrl,
                Following = this.Following,
                Followed = this.Followed,
                Posts = this.Posts,
                Id = this.Id,
                AuthToken = this.AuthToken,
                SocialData = this.SocialData
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the ShippingAddress classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericShippingAddress {
        [StringLength(12)]
        public string ProfileID { get; set; } //profile ID
        [StringLength(65)]
        public string FirstName { get; set; } //first name
        [StringLength(65)]
        public string MiddleName { get; set; } //middle name
        [StringLength(65)]
        public string Lastname { get; set; } //last name
        [StringLength(100)]
        public string StreetName { get; set; } //shipping street
        [StringLength(100)]
        public string Streetname2 { get; set; } //shipping street second line
        [StringLength(5)]
        public string HouseNumber { get; set; } //
        [StringLength(5)]
        public string HouseExtension { get; set; } //
        [StringLength(50)]
        public string City { get; set; } //shipping city
        [StringLength(50)]
        public string ZipCode { get; set; } //shipping zip code
        [StringLength(50)]
        public string State { get; set; } //shipping state
        [StringLength(2)]
        public string CountryCode { get; set; } //alpha-2 country code (see CodeTables.ISOCountryCodes)
        [StringLength(100)]
        public string Email { get; set; } //shipping contact email
        [StringLength(20)]
        public string PrimaryPhone { get; set; } //shipping primary phone
        [StringLength(20)]
        public string SecondaryPhone { get; set; } //shipping secondary phone
        public string Company { get; set; }
        public string StateCode { get; set; }

        /// <summary>
        /// This method computes the object used to provide shipping address details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ShippingAddress TestVersion() {
            return new CryptDecryptTest.ShippingAddress {
                ProfileID = this.ProfileID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                StreetName = this.StreetName,
                Streetname2 = this.Streetname2,
                HouseNumber = this.HouseNumber,
                HouseExtention = this.HouseExtension,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                CountryCode = this.CountryCode,
                Email = this.Email,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                Company = this.Company,
                StateCode = this.StateCode
            };
        }
        /// <summary>
        /// This method computes the object used to provide shipping address details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ShippingAddress ProdVersion() {
            return new CryptDecryptProd.ShippingAddress {
                ProfileID = this.ProfileID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                StreetName = this.StreetName,
                Streetname2 = this.Streetname2,
                HouseNumber = this.HouseNumber,
                HouseExtention = this.HouseExtension,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                CountryCode = this.CountryCode,
                Email = this.Email,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                Company = this.Company,
                StateCode = this.StateCode
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the BillingAddress classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericBillingAddress {
        [StringLength(12)]
        public string ProfileID { get; set; } //profile ID
        [StringLength(65)]
        public string FirstName { get; set; } //first name
        [StringLength(65)]
        public string MiddleName { get; set; } //middle name
        [StringLength(65)]
        public string Lastname { get; set; } //last name
        [StringLength(100)]
        public string StreetName { get; set; } //shipping street
        [StringLength(100)]
        public string Streetname2 { get; set; } //shipping street second line
        [StringLength(5)]
        public string HouseNumber { get; set; } //
        [StringLength(5)]
        public string HouseExtension { get; set; } //
        [StringLength(50)]
        public string City { get; set; } //billing city
        [StringLength(50)]
        public string ZipCode { get; set; } //billing zip code
        [StringLength(50)]
        public string State { get; set; } //billing state
        [StringLength(2)]
        public string CountryCode { get; set; } //alpha-2 country code (see CodeTables.ISOCountryCodes)
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(20)]
        public string PrimaryPhone { get; set; } //
        [StringLength(20)]
        public string SecondaryPhone { get; set; } //
        public string Company { get; set; }
        public string StateCode { get; set; }

        /// <summary>
        /// This method computes the object used to provide billing address details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.BillingAddress TestVersion() {
            return new CryptDecryptTest.BillingAddress {
                ProfileID = this.ProfileID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                StreetName = this.StreetName,
                Streetname2 = this.Streetname2,
                HouseNumber = this.HouseNumber,
                HouseExtention = this.HouseExtension,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                CountryCode = this.CountryCode,
                Email = this.Email,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                Company = this.Company,
                StateCode = this.StateCode
            };
        }
        /// <summary>
        /// This method computes the object used to provide billing address details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.BillingAddress ProdVersion() {
            return new CryptDecryptProd.BillingAddress {
                ProfileID = this.ProfileID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                StreetName = this.StreetName,
                Streetname2 = this.Streetname2,
                HouseNumber = this.HouseNumber,
                HouseExtention = this.HouseExtension,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                CountryCode = this.CountryCode,
                Email = this.Email,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                Company = this.Company,
                StateCode = this.StateCode
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the ProductDetail classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericProductDetail {
        [StringLength(12)]
        public string ProductCode { get; set; } //Article's product code
        [StringLength(50)]
        public string SKU { get; set; } //article's stock keeping unit
        [StringLength(100)]
        public string Name { get; set; } //article's name
        [StringLength(255)]
        public string Description { get; set; } //article's description
        [StringLength(3)]
        public string Quantity { get; set; } //the number of products
        [StringLength(12)]
        public string Price { get; set; } //the number of products
        [StringLength(12)]
        public string UnitPrice { get; set; } //article's unit price
        [StringLength(2)]
        public string Type { get; set; } //the type of article: 1-product,, 2-shipping, 3-handling
        [StringLength(2)]
        public string Vat { get; set; } //value added tax (the value of the tax)
        [StringLength(2)]
        public string Discount { get; set; } //the amount offered by you as discount
        public string RequiresShipping { get; set; }
        public string Condition { get; set; }
        public string Seller { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Brand { get; set; }
        public string DeliveryAt { get; set; }

        /// <summary>
        /// This method computes the object used to provide product details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ProductDetail TestVersion() {
            return new CryptDecryptTest.ProductDetail {
                ProductCode = this.ProductCode,
                SKU = this.SKU,
                Name = this.Name,
                Description = this.Description,
                Quantity = this.Quantity,
                Price = this.Price,
                UnitPrice = this.UnitPrice,
                Type = this.Type,
                Vat = this.Vat,
                Discount = this.Discount,
                RequiresShipping = this.RequiresShipping,
                Condition = this.Condition,
                Seller = this.Seller,
                Category = this.Category,
                SubCategory = this.SubCategory,
                Brand = this.Brand,
                DeliveryAt = this.DeliveryAt
            };
        }
        /// <summary>
        /// This method computes the object used to provide product details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ProductDetail ProdVersion() {
            return new CryptDecryptProd.ProductDetail {
                ProductCode = this.ProductCode,
                SKU = this.SKU,
                Name = this.Name,
                Description = this.Description,
                Quantity = this.Quantity,
                Price = this.Price,
                UnitPrice = this.UnitPrice,
                Type = this.Type,
                Vat = this.Vat,
                Discount = this.Discount,
                RequiresShipping = this.RequiresShipping,
                Condition = this.Condition,
                Seller = this.Seller,
                Category = this.Category,
                SubCategory = this.SubCategory,
                Brand = this.Brand,
                DeliveryAt = this.DeliveryAt
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the DiscountCode classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericDiscountCode {
        public string Amount { get; set; }
        public string Code { get; set; }

        /// <summary>
        /// This method computes the object used to provide discount code details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.DiscountCode TestVersion() {
            return new CryptDecryptTest.DiscountCode {
                Amount = this.Amount,
                Code = this.Code
            };
        }
        /// <summary>
        /// This method computes the object used to provide discount code details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.DiscountCode ProdVersion() {
            return new CryptDecryptProd.DiscountCode {
                Amount = this.Amount,
                Code = this.Code
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the ShippingLine classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericShippingLine {
        public string Price { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }

        /// <summary>
        /// This method computes the object used to provide shipping line details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ShippingLine TestVersion() {
            return new CryptDecryptTest.ShippingLine {
                Price = this.Price,
                Title = this.Title,
                Code = this.Code
            };
        }
        /// <summary>
        /// This method computes the object used to provide shipping line details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ShippingLine ProdVersion() {
            return new CryptDecryptProd.ShippingLine {
                Price = this.Price,
                Title = this.Title,
                Code = this.Code
            };
        }
    }

}