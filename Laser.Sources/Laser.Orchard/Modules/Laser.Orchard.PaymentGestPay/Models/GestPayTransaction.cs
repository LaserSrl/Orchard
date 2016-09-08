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
        [StringLength(50)]
        public string buyerName { get; set; } //Buyer's name and surname
        [StringLength(50)]
        public string buyerEmail { get; set; } //Buyer's email address
        [StringLength(2)]
        public string languageId { get; set; } //Code identifying language used in communication with buyer
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
        public GenericShippingDetails Red_ShippingInfo { get; set; }
        public GenericRedBillingInfo Red_BillingInfo { get; set; }
        public GenericRedCustomerData Red_CustomerData { get; set; }
        public string[] Red_CustomInfo { get; set; }
        public GenericRedItems Red_Items { get; set; }
        [StringLength(3)]
        public string Consel_MerchantPro { get; set; } //merchant promotional code (mandatory to show consel in the pagam's payment method)
        public GenericConselCustomerInfo Consel_CustomerInfo { get; set; }
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
}