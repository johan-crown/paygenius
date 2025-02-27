using Newtonsoft.Json;
using PayGenius.Requests;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PayGenius
{
    //PaygeniusMainClient client = new PaygeniusMainClient("https://developer.paygenius.co.za/", "b667631c-5505-4a8a-b6cd-14ab024763e0", "2a26f945-43b2-4ddb-90b4-55b05205d991");

    public class PayGeniusMainClient
    {
        private string token;
        private string secret;
        private string endpoint = "https://developer.paygenius.co.za/";

        public PayGeniusMainClient(string token, string secret)
        {
            this.token = token;
            this.secret = secret;
        }

        public string Validate(Validate request)
        {
            return this.Send(request);
        }

        public CreatePaymentForTokenResponse CreatePaymentToken(CreatePaymentForTokenRequest request)
        {
            var result = this.Send(request);

            var response = JsonConvert.DeserializeObject<CreatePaymentForTokenResponse>(result);

            return response;
        }

        public CardLookupResponse LookupCardByToken(CardLookupByTokenRequest request)
        {
            var result = this.Send(request);
            var response = JsonConvert.DeserializeObject<CardLookupResponse>(result);
            return response;
        }

        public CardLookupResponse LookupCardByCardNumber(CardLookupByCardNumberRequest request)
        {
            var result = this.Send(request);
            var response = JsonConvert.DeserializeObject<CardLookupResponse>(result);
            return response;
        }

        public string Transaction(TransactionRequest request)
        {
            return this.Send(request);
        }

        public StoreCardResponse StoreCard(StoreCardRequest request)
        {
            var result = this.Send(request);
            var response = JsonConvert.DeserializeObject<StoreCardResponse>(result);
            return response;
        }

        public string DeleteCard(UnregisterCardRequest request)
        {
            return this.Send(request);
        }

        public ConfirmPaymentResponse ConfirmPayment(ConfirmPaymentRequest request)
        {
            var result = this.Send(request);
            ConfirmPaymentResponse response = JsonConvert.DeserializeObject<ConfirmPaymentResponse>(result);

            return response;
        }

        public string ConfirmPayment(Confirm3dsV2PaymentRequest request)
        {
            var result = this.Send(request);

            return result;
        }


        public CreatePaymentForTokenResponse CreateRecurringPayment(RecurringPaymentRequest request)
        {
            var result = this.Send(request);
            var response = JsonConvert.DeserializeObject<CreatePaymentForTokenResponse>(result);
            return response;
        }

        public string RenewRecurringPayment(RenewRecurringRequest request)
        {
            return this.Send(request);
        }

        public string TransactionLookup(TransactionLookupRequest request)
        {
            return this.SendGet(request);
        }

        public PaymentWithSubscriptionResponse CreateSubscription(PaymentWithSubscriptionRequest request)
        {
            var result = this.Send(request);

            var response = JsonConvert.DeserializeObject<PaymentWithSubscriptionResponse>(result);

            return response;
        }

        public string AffiliateMerchantLookup(MerchantLookupRequest request)
        {
            return this.SendGet(request);
        }

        public string Send(AbstractRequest request)
        {
            string data = JsonConvert.SerializeObject(request);
            string fullEndpoint = $"{this.endpoint.TrimEnd('/')}/{request.Endpoint.TrimStart('/')}";
            var nullJsonCheck = data.Replace("{", "").Replace("}", "").Trim();
            var signature = this.Sign(fullEndpoint, String.IsNullOrEmpty(nullJsonCheck) ? null : data);

            using (var httpClient = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri(fullEndpoint),
                    Method = String.IsNullOrEmpty(nullJsonCheck) ? HttpMethod.Get : HttpMethod.Post,
                };

                httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                httpRequest.Headers.Add("X-Token", token);
                httpRequest.Headers.Add("X-Signature", signature);

                if (!String.IsNullOrEmpty(nullJsonCheck))
                {
                    httpRequest.Content = new StringContent(data, Encoding.UTF8, "application/json");
                }

                var httpResponse = httpClient.Send(httpRequest);
                httpResponse.EnsureSuccessStatusCode();

                return httpResponse.Content.ReadAsStringAsync().Result;
            }
        }


        /// <summary>
        /// Used to send GET requests as they fail when sent through the Send method
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string SendGet(AbstractRequest request)
        {
            string data = JsonConvert.SerializeObject(request);
            string fullEndpoint = this.endpoint + request.Endpoint;
            var signiture = this.Sign(fullEndpoint, null);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(fullEndpoint);
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Headers["X-Token"] = token;
            httpWebRequest.Headers["X-Signature"] = signiture;
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }

        public string Sign(string endpoint, string data = null)
        {
            string toSign = endpoint;
            if (data != null)
            {
                toSign += "\n" + data;
            }

            HMACSHA256 hmac = new HMACSHA256(System.Text.Encoding.Default.GetBytes(this.secret));

            byte[] hash = hmac.ComputeHash(System.Text.Encoding.Default.GetBytes(toSign.Trim()));

            return ByteToString(hash);
        }

        public string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2");
            }
            return sbinary.ToLower();
        }

    }


    public class CreditCardInfo
    {
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CVV { get; set; }
        public string CardHolderName { get; set; }
    }

}

namespace PayGenius.Requests
{

    public class AbstractRequest
    {
        protected string endpoint { get; set; }
        protected string method { get; set; }

        public AbstractRequest(string endpoint = null, string method = null)
        {
            this.endpoint = endpoint;
            this.method = method;
        }

        public string Endpoint => this.endpoint;

        public string Method => this.method;
    }



    public class CardLookupByTokenRequest : AbstractRequest
    {
        public string token;
        public CardLookupByTokenRequest(string token)
        {
            this.token = token;
            this.endpoint = "pg/api/v2/card/lookup";
            this.method = "POST";
        }
    }

    public class CardLookupByCardNumberRequest : AbstractRequest
    {
        public string cardNumber;
        public CardLookupByCardNumberRequest(string cardNumber)
        {
            this.cardNumber = cardNumber;
            this.endpoint = "pg/api/v2/card/lookup";
            this.method = "POST";
        }
    }

    public class CardLookupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public CardDetails Card { get; set; }

    }

    public class CardDetails
    {
        public string Number { get; set; }
        public string CardHolder { get; set; }
        public string ExpiryYear { get; set; }
        public string ExpiryMonth { get; set; }
        public string Cvv { get; set; }
        public string Token { get; set; }
    }

    public class ConfirmPaymentRequest : AbstractRequest
    {
        protected string reference;
        public string paRes;

        public ConfirmPaymentRequest(string reference, string paRes)
        {
            this.reference = reference;
            this.paRes = paRes;
            this.method = "POST";
            this.endpoint = $"pg/api/v2/payment/{this.reference}/confirm";
        }
    }

    public class Confirm3dsV2PaymentRequest : AbstractRequest
    {
        private string reference;
        private string txId;
        private string transactionId;

        public Confirm3dsV2PaymentRequest(string reference, string txId, string transactionId)
        {
            Reference = reference;
            TxId = txId;
            TransactionId = transactionId;
            method = "POST";
            endpoint = $"/pg/api/v2/payment/3dsv2/{reference}/confirm";
        }

        [JsonProperty("transactionId")]
        public string TransactionId { get => transactionId; set => transactionId = value; }

        [JsonProperty("reference")]
        public string Reference { get => reference; set => reference = value; }

        [JsonProperty("txId")]
        public string TxId { get => txId; set => txId = value; }
    }

    public class Confirm3dsV2PaymentResponse : ConfirmPaymentResponse
    {
        public bool success { get; set; }
        public DateTime transactionDate { get; set; }
        public string message { get; set; }
        public Threedsecurev2 threeDSecureV2 { get; set; }
        public int code { get; set; }
        public string reference { get; set; }

        public Confirm3dsV2PaymentResponse()
        {

        }
    }

    public class ConfirmPaymentResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string originalErrorCode { get; set; }
        public int code { get; set; }
    }

    public class CreatePaymentForTokenRequest : AbstractRequest
    {
        public CreatePaymentForTokenRequest()
        {
            this.endpoint = "pg/api/v2/payment/create";
            this.method = "POST";
        }

        public bool threeDSecure { get; set; }
        public bool supports3dsV2 { get; set; }
        public bool autoExecute { get; set; }

        public CreditCard creditCard { get; set; } = new CreditCard();

        public Transaction transaction { get; set; } = new Transaction();

        public string callbackUrl { get; set; }

        /// <summary>
        /// Create once off payment
        /// </summary>
        /// <param name="token">card token</param>
        /// <param name="reference">local transaction reference</param>
        /// <param name="currency">currency</param>
        /// <param name="amount">payment amount</param>
        /// <param name="threeDSecure">Use 3D Secure auth for purchase</param>
        /// <param name="supports3dsV2">Supports 3D Secure auth V2</param>
        /// <param name="callbackUrl">3D Secure auth callback url</param>
        public CreatePaymentForTokenRequest(string token, string reference, string currency, decimal amount, bool threeDSecure = true, bool supports3dsV2 = true, string callbackUrl = null)
        {
            this.endpoint = "pg/api/v2/payment/create";
            this.method = "POST";
            this.creditCard.token = token;
            this.transaction.reference = reference;
            this.transaction.currency = currency;
            this.supports3dsV2 = supports3dsV2;
            this.transaction.amount = (int)(amount * 100);//converting to cents as paygenius expects
            this.threeDSecure = threeDSecure;
            this.callbackUrl = callbackUrl;
            this.autoExecute = false;
        }
    }

    public class CreditCard
    {
        public string token { get; set; }

        public string number { get; set; }
        public string cardHolder { get; set; }
        public string expiryYear { get; set; }
        public string expiryMonth { get; set; }
        public string cvv { get; set; }
        public string type { get; set; }
        public string uniqueId { get; set; }
        public string paymentReference { get; set; }
        public string transactionReference { get; set; }
    }

    public class Transaction
    {
        public string reference { get; set; }
        public string currency { get; set; }
        /// <summary>
        /// NOTE AMOUNT IS REQUIRED IN CENTS
        /// </summary>
        public int amount { get; set; }
    }

    public class CreatePaymentForTokenResponse
    {

        public bool Success { get; set; }
        public Secure3DResponse threeDSecure { get; set; }
        public Threedsecurev2 threeDSecureV2 { get; set; }
        public string reference { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }

        public bool success { get; set; }
        public DateTime transactionDate { get; set; }
    }

    public class Threedsecurev2
    {
        public string tdsMethodContent { get; set; }
        public string txId { get; set; }
        public string transactionId { get; set; }
        public object sessionData { get; set; }
        public object acsUrl { get; set; }
        public object creq { get; set; }
    }


    //3D secure response
    public class Secure3DResponse
    {
        public string acsUrl { get; set; }

        public string transactionId { get; set; }

        public string paReq { get; set; }
    }

    public class Consumer
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
    }

    public class CreatePaymentRequest : AbstractRequest
    {
        public CreditCard creditCard;
        public Consumer consumer;
        public bool threeDSecure;
        public Transaction transaction;

        public CreatePaymentRequest(CreditCard creditCard, Transaction transaction, bool threeDSecure, Consumer consumer)
        {
            this.endpoint = "pg/api/v2/payment/create";
            this.method = "POST";

            this.creditCard = creditCard;
            this.consumer = consumer;
            this.transaction = transaction;
            this.threeDSecure = threeDSecure;
        }
    }

    public class ExecutePaymentRequest : AbstractRequest
    {
        public string reference;

        public ExecutePaymentRequest(string reference)
        {
            this.reference = reference;
            this.endpoint = $"pg/api/v2/payment/{reference}/execute";
            this.method = "GET";
        }

        public string getEndpoint()
        {
            return string.Format(this.endpoint, this.reference);

        }
    }

    public class ExecutePaymentResponse
    {
        public bool success { get; set; }

        public string message { get; set; }

        public string originalErrorCode { get; set; }

        public int code { get; set; }
    }

    public class StoreCardRequest : AbstractRequest
    {

        public string number;
        public string cardHolder;
        public int expiryYear;
        public int expiryMonth;
        public string cvv;
        public string type;

        public StoreCardRequest(string number, string cardHolder, int expiryYear, int expiryMonth, string cvv)
        {
            this.endpoint = "pg/api/v2/card/register";
            this.method = "POST";
            this.number = number;
            this.cardHolder = cardHolder;
            this.expiryYear = expiryYear;
            this.expiryMonth = expiryMonth;
            this.cvv = cvv;
        }
    }


    public class StoreCardResponse
    {
        public bool Success { get; set; }

        public string Token { get; set; }

        public string Message { get; set; }
    }

    public class TransactionRequest : AbstractRequest
    {

        //  public string merchantReference;
        public string paymentReference;
        //public string fromDate;
        /// <summary>
        /// NOTE AMOUNT IS REQUIRED IN CENTS
        /// </summary>
        public int amount;
        //  public int merchantid;
        public string status;
        public string currency;


        public TransactionRequest(string paymentReference, int amount, string status, string currency)
        {
            this.endpoint = "pg/api/v2/payment/list";
            this.method = "POST";


            //this.merchantReference = merchantReference;
            this.paymentReference = paymentReference;
            //this.fromDate = fromDate;
            this.amount = amount * 100;//converting to cents as paygenius expects
            //this.merchantid = merchantid;
            this.status = status;
            this.currency = currency;


        }

    }

    public class TransactionResponse
    {
        public bool successs { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public List<TransactionDetails> transactions { get; set; } = new List<TransactionDetails>();
    }

    public class TransactionDetails
    {
        public int id { get; set; }
        public string reference { get; set; }
        public string currency { get; set; }
        public int amount { get; set; }
        public string date { get; set; }
        public string source { get; set; }
        public string merchantReference { get; set; }
        public string paymentMethod { get; set; }
        public string message { get; set; }
        public bool suspended { get; set; }
        public string merchant { get; set; }
        public int fees { get; set; }

        public int feesVat { get; set; }
        public string consumerEmail { get; set; }
    }

    public class UnregisterCardRequest : AbstractRequest
    {

        public string token;

        public UnregisterCardRequest()
        {
        }

        public UnregisterCardRequest(string token)
        {
            this.endpoint = "pg/api/v2/card/unregister";
            this.method = "POST";

            this.token = token;

        }
    }

    public class UnregisterCardResponse
    {
        public bool Success { get; set; }
    }

    public class Validate : AbstractRequest
    {
        public Validate()
        {
            this.endpoint = "pg/api/v2/util/validate";
            this.method = "GET";
        }
    }

    public class ValidateResponse
    {
        public bool success { get; set; }
        public string merchant { get; set; }
    }

    public class Confirm3DSecureModel
    {
        public string AcsUrl { get; set; }

        public string TermUrl { get; set; }//this is the callback url the bank will send the response to

        public string MD { get; set; }//transactionId

        public string connector { get; set; } = "THREEDSECURE";

        public string PaReq { get; set; } //paReq value provided in CreatePaymentForTokenResponse

        public int InvoiceId { get; set; }

        public Confirm3DSecureModel(string acsUrl, string termUrl, string mD, string paReq)
        {
            AcsUrl = acsUrl;
            TermUrl = termUrl;
            MD = mD;
            PaReq = paReq;
        }
    }

    public class RecurringPaymentRequest : AbstractRequest
    {
        /// <summary>
        /// initial payment requires cvv
        /// </summary>
        public CreditCard creditCard { get; set; } = new CreditCard();

        public Transaction transaction { get; set; } = new Transaction();

        public bool threeDSecure { get; set; } = true;

        public bool recurring { get; set; } = true;

        public bool supports3dsV2 { get; set; } = false;

        public string callbackUrl { get; set; }

        private string clientIP { get; set; }
        public int screenWidth { get; }
        public int screenHeight { get; }

        /// <summary>
        /// Create recuring payment initial payment
        /// </summary>
        /// <param name="cardToken">Card token</param>
        /// <param name="cardCvv">card cvv</param>
        /// <param name="Reference">local payment reference</param>
        /// <param name="transactionAmount"> Transaction Amount</param>
        /// <param name="threeDSecure"><User 3D Secure/param>
        /// <param name="supports3dsV2">Supports 3D Secure v2</param>
        /// <param name="callbackUrl">3D Secure callback url</param>
        public RecurringPaymentRequest(string cardToken, string cardCvv, string Reference, int transactionAmount, string clientIP, bool threeDSecure = true, bool supports3dsV2 = true, string callbackUrl = null)
        {
            this.endpoint = "pg/api/v2/payment/create";
            this.method = "POST";
            creditCard.token = cardToken;
            creditCard.cvv = cardCvv;
            transaction.reference = Reference;
            transaction.currency = "ZAR";
            transaction.amount = transactionAmount * 100;//converting to cents as paygenius expects
            this.threeDSecure = threeDSecure;
            this.supports3dsV2 = supports3dsV2;
            this.callbackUrl = callbackUrl;
            this.clientIP = clientIP;
            screenWidth = 1024;
            screenHeight = 600;
        }
    }

    public class Subscription
    {
        public string reference { get; set; }

        /// <summary>
        /// Possible values: MONTHLY, DAILY, YEARLY
        /// </summary>
        public string interval { get; set; }

        public int trailDays { get; set; }

        public string email { get; set; }

        public string firstname { get; set; }

        public string lastname { get; set; }
    }

    public class RenewRecurringRequest : AbstractRequest
    {
        public CreditCard creditCard { get; set; } = new CreditCard();

        public Transaction transaction { get; set; } = new Transaction();

        public bool threeDSecure { get; set; } = false;

        public bool supports3dsV2 { get; set; } = true;

        public bool recurring { get; set; } = true;

        public string initialRef { get; set; }

        public string callbackUrl { get; set; }

        public RenewRecurringRequest(string paymentReference, string Reference, int transactionAmount, string token, string cvv, string callbackUrl)
        {
            this.endpoint = "pg/api/v2/payment/create";
            this.method = "POST";
            transaction.reference = Reference;
            transaction.currency = "ZAR";
            transaction.amount = transactionAmount * 100;//converting to cents
            this.threeDSecure = threeDSecure;
            initialRef = paymentReference;
            creditCard.token = token;
            creditCard.cvv = cvv;
            this.callbackUrl = callbackUrl;
        }
    }

    public class TransactionLookupRequest : AbstractRequest
    {
        public TransactionLookupRequest(string reference)
        {
            this.endpoint = $"pg/api/v2/payment/{reference}";
            this.method = "GET";
        }
    }

    public class TransactionLookupResponse
    {
        public bool Success { get; set; }

        public string Reference { get; set; }

        /// <summary>
        /// NOTE AMOUNT IS RETURNED IN CENTS
        /// </summary>
        public int Amount { get; set; }

        public string Currency { get; set; }

        public string Status { get; set; }

        /* Status explanations
        AUTHORIZED	    Funds have been reserved on the user's account
        SETTLED	        The transaction has been completed
        CANCELLED	    The transaction was cancelled by the user
        REFUNDED	    A completed refund transaction
        THREE_D_SECURE	Awaiting 3D secure confirmation
        FAILED	        Indicates a failed transaction
        REVERSED	    The transaction has been reversed
         */
    }

    public class PaymentWithSubscriptionRequest : AbstractRequest
    {
        public CreditCard creditCard { get; set; } = new CreditCard();

        public Transaction transaction { get; set; } = new Transaction();

        public bool threeDSecure { get; set; }

        public Subscription subscription { get; set; } = new Subscription();

        /// <summary>
        /// Request to create new subscription for user
        /// </summary>
        /// <param name="cardToken">card token</param>
        /// <param name="purchaseReference">local transaction reference</param>
        /// <param name="amount">amount</param>
        /// <param name="userEmail">Email of user subscription is made for</param>
        /// <param name="firstname">user linked to subscription first name</param>
        /// <param name="lastname">user linked to subscription last name</param>
        /// <param name="interval">Possible values: "MONTHLY", "DAILY", "YEARLY"</param>
        /// <param name="threeDSecure">use 3D Secure Auth</param>
        /// <param name="trailDays">amount of days before first payment made</param>
        public PaymentWithSubscriptionRequest(string cardToken, string purchaseReference, double amount, string userEmail, string firstname, string lastname, string interval = "MONTHLY", bool threeDSecure = true, int trailDays = 0)
        {
            this.endpoint = "pg/api/v2/payment/create";
            this.method = "POST";
            transaction.currency = "ZAR";
            creditCard.token = cardToken;
            transaction.reference = purchaseReference;
            subscription.reference = purchaseReference;
            transaction.amount = (int)(amount * 100);//converting to cents as PayGenius expects
            subscription.email = userEmail;
            subscription.firstname = firstname;
            subscription.lastname = lastname;
            subscription.interval = interval;
            this.threeDSecure = threeDSecure;
            subscription.trailDays = trailDays;
        }
    }

    public class MerchantLookupRequest : AbstractRequest
    {
        public MerchantLookupRequest()
        {
            this.endpoint = "pg/api/v2/transfer/merchants/get";
            this.method = "GET";
        }
    }


    public class PaymentWithSubscriptionResponse
    {
    }
}
