using System;
using System.Configuration;
using System.Net;
using Coinbase.ObjectModel;
using Coinbase.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace Coinbase
{
	public static class CoinbaseUrls
	{
		public const string LiveApiUrl = "https://api.coinbase.com/v1/";
		public const string TestApiUrl = "https://api.sandbox.coinbase.com/v1/";
		public const string LiveCheckoutUrl = "https://coinbase.com/checkouts/{code}";
		public const string TestCheckoutUrl = "https://sandbox.coinbase.com/checkouts/{code}";
	}
    public class CoinbaseApi
    {
        private readonly string apiKey;
        private readonly string apiSecret;
	    private readonly string apiUrl;
	    private readonly string apiCheckoutUrl;
	    private readonly WebProxy proxy;

        private JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
		

        [Obsolete( "Simple API Keys are being deprecated in favor of the new API Key + Secret system. Read more in the API docs.", true )]
        public CoinbaseApi(string apiSimpleKey)
        {
            throw new InvalidOperationException( "Simple API Keys are being deprecated in favor of the new API Key + Secret system. Read more in the API docs." );
        }

	    public CoinbaseApi(string apiKey = "", string apiSecret = "", bool useSandbox = false, WebProxy proxy = null) :
			this( apiKey, apiSecret, useSandbox ? CoinbaseUrls.TestApiUrl : null, useSandbox ? CoinbaseUrls.TestCheckoutUrl : null, proxy )
	    {
		    
	    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="apiKey">Your API Key</param>
		/// <param name="apiSecret">Your API Secret </param>
		/// <param name="customApiEndpoint">A custom URL endpoint. Typically, you'd use this if you want to use the sandbox URL.</param>
        public CoinbaseApi( 
			string apiKey,
			string apiSecret,
			string apiUrl = CoinbaseUrls.LiveApiUrl,
			string checkoutUrl = CoinbaseUrls.LiveCheckoutUrl,
			WebProxy proxy = null)
        {
            this.apiKey = !string.IsNullOrWhiteSpace( apiKey ) ? apiKey : ConfigurationManager.AppSettings["CoinbaseApiKey"];
            this.apiSecret = !string.IsNullOrWhiteSpace( apiSecret ) ? apiSecret : ConfigurationManager.AppSettings["CoinbaseApiSecret"];
            if ( string.IsNullOrWhiteSpace( this.apiKey ) || string.IsNullOrWhiteSpace(this.apiSecret) )
            {
                throw new ArgumentException( "The API key / secret must not be empty. A valid API key and API secret should be used in the CoinbaseApi constructor or an appSettings configuration element with <add key='CoinbaseApiKey' value='my_api_key' /> and <add key='CoinbaseApiSecret' value='my_api_secret' /> should exist.", "apiKey" );
            }
			this.apiUrl = apiUrl;
			this.apiCheckoutUrl = checkoutUrl;
			this.proxy = proxy;
        }

        public CoinbaseApi(string apiKey, string apiSecret, JsonSerializerSettings settings) : this(apiKey, apiSecret, useSandbox:false)
        {
            this.settings = settings;
        }

        protected virtual RestClient CreateClient()
        {          
            var client = new RestClient( apiUrl )
	            {
		            Proxy = this.proxy,
					Authenticator = GetAuthenticator()
	            };

	        client.AddHandler( "application/json", new JsonNetDeseralizer( settings ) );
            return client;
        }

        protected virtual IAuthenticator GetAuthenticator()
        {
            return new CoinbaseApiAuthenticator(apiKey, apiSecret);
        }

        protected virtual IRestRequest CreateRequest( string action, Method method = Method.POST )
        {
            var post = new RestRequest(action, method)
                {
                    RequestFormat = DataFormat.Json,
                    JsonSerializer = new JsonNetSerializer(settings),
                };               

            return post;
        }

        /// <summary>
        /// Authenticated resource that creates a payment button, page, or iFrame to accept bitcoin on your website. This can be used to accept bitcoin for an individual item or to integrate with your existing shopping cart solution. For example, you could create a new payment button for each shopping cart on your website, setting the total and order number in the button at checkout.
        ///
        /// The code param returned in the response can be used to generate an embeddable button, payment page, or iFrame.
        /// </summary>
        public virtual ButtonResponse RegisterButton(ButtonRequest buttonRequest)
        {
            var client = CreateClient();

            var post = CreateRequest( "buttons" )
                .AddBody( buttonRequest );
            
            var resp = client.Execute<ButtonResponse>( post );
			
            if ( resp.ErrorException != null )
                throw resp.ErrorException;
            if ( resp.ErrorMessage != null )
                throw new Exception(resp.ErrorMessage);

	        resp.Data.CheckoutPageUrl = this.apiCheckoutUrl;

            return resp.Data;
        }

        /// <summary>
        /// Authenticated resource which lets you generate an order associated with a button. After generating an order, you can send bitcoin to the address associated with the order to complete the order. The status of this newly created order will be ‘new’.
        /// </summary>
        public OrderResponse CreateOrder( ButtonResponse response )
        {
            var code = response.Button.Code;

            var client = CreateClient();

            var post = CreateRequest( "buttons/{code}/create_order" )
                .AddUrlSegment( "code", code );

            var resp = client.Execute<OrderResponse>(post);

            if ( resp.ErrorException != null )
                throw resp.ErrorException;

            return resp.Data;
        }

        /// <summary>
        /// Authenticated resource which refunds an order or a mispayment to an order. Returns a snapshot of the order data, updated with refund transaction details.
        /// This endpoint will only refund the full amount of the order or mispayment, specified as either the original BTC amount or native currency amount (such as USD). To issue partial refunds, you can use the regular api/v1/transactions/send_money endpoint.
        /// https://www.coinbase.com/api/doc/1.0/orders/refund.html
        /// </summary>
        /// <param name="orderToken">id_or_custom_field URL parameter</param>
        /// <param name="refundOptions">Refund options for this order ID</param>
        /// <returns>If the order has status completed and the refund processed successfully, the order data will contain the refund transaction details in RefundTransaction. If the refund does not process, order['errors'] will be present, specifying any problems.</returns>
        public RefundResponse Refund(string orderToken, RefundOptions refundOptions)
        {
            var client = CreateClient();
            var body = new RefundRequest()
                {
                    RefundOptions = refundOptions
                };
            var post = CreateRequest("orders/{orderId}/refund")
                .AddUrlSegment("orderId", orderToken)
                .AddBody(body);

            var resp = client.Execute<RefundResponse>(post);

            if (resp.ErrorException != null)
                throw resp.ErrorException;

            if( resp.Data.Order != null )
            {
                resp.Data.Success = true;
            }

            return resp.Data;
        }

        /// <summary>
        /// Authenticated resource which lets you send money to an email or bitcoin address. https://www.coinbase.com/api/doc/1.0/transactions/send_money.html
        /// </summary>
        /// <param name="to">An email address or a bitcoin address</param>
        /// <param name="amount">A string amount that will be converted to BTC, such as ‘1’ or ‘1.234567’. Also must be >= ‘0.01’ or it will shown an error.</param>
        /// <param name="notes">Optional notes field. Included in the email that the recipient receives.</param>
        /// <returns></returns>
        public SendMoneyResponse SendMoney(Payment payment)
        {
            var sendMoneyRequest = new SendMoneyRequest()
                {
                    transaction = payment
                };

            var client = CreateClient();
            var post = CreateRequest("transactions/send_money").AddBody(sendMoneyRequest);
            var resp = client.Execute<SendMoneyResponse>(post);


            if (resp.ErrorException != null)
                throw resp.ErrorException;

            return resp.Data;
        }

        /// <summary>
        /// Authenticated resource which returns order details. 
        /// See: https://coinbase.com/api/doc/1.0/orders/show.html
        /// If an order has received multiple payments (i.e., in the case of mispayments), this call will return an array that lists these.
        /// </summary>
        /// <param name="orderToken">You can pass in the order id (a Coinbase field) or custom (a merchant field) to find the appropriate order.</param>
        /// <returns></returns>
        public GetOrderResponse GetOrder(string orderToken)
        {
            var client = CreateClient();

            var post = CreateRequest("orders/{orderId}", Method.GET)
                        .AddUrlSegment("orderId", orderToken);

            var resp = client.Execute<GetOrderResponse>(post);

            if (resp.ErrorException != null)
                throw resp.ErrorException;

            return resp.Data;
        }
    }
}