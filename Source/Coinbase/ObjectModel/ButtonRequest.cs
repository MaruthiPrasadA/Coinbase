using Coinbase.Converters;
using FluentValidation.Attributes;
using Newtonsoft.Json;

namespace Coinbase.ObjectModel
{
    /// <summary>
    /// Authenticated resource that creates a payment button, page, or iFrame to accept bitcoin on your website. This can be used to accept bitcoin for an individual item or to integrate with your existing shopping cart solution. For example, you could create a new payment button for each shopping cart on your website, setting the total and order number in the button at checkout.
    ///
    /// The code param returned in the response can be used to generate an embeddable button, payment page, or iFrame.
    /// </summary>
    [Validator(typeof(ButtonValidator))]
    public class ButtonRequest
    {
        /// <summary>
        /// The name of the item for which you are collecting bitcoin. For example, Acme Order #123 or Annual Pledge Drive
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// One of buy_now or donation. Default is buy_now.
        /// </summary>
        public ButtonType Type { get; set; }

        /// <summary>
        /// Whether or not this button is a subscription.
        /// </summary>
        public bool Subscription { get; set; }

        /// <summary>
        /// Required if 'Subscription' is true. Must be one of never, hourly, daily, weekly, every_two_weeks, monthly, quarterly, and yearly.
        /// </summary>
        public SubscriptionType? Repeat { get; set; }

        /// <summary>
        /// Price as a decimal string, for example 1.23. Can be more then two significant digits if price_currency_iso equals BTC.
        /// </summary>
        [JsonProperty("price_string")]
        [JsonConverter(typeof(DecimalStringConverter))]
        public decimal Price { get; set; }

        /// <summary>
        /// Price currency as an ISO 4217 code such as USD or BTC. This determines what currency the price is shown in on the payment widget.
        /// </summary>
        [JsonProperty("price_currency_iso")]
        public Currency Currency { get; set; }

        /// <summary>
        /// An optional custom parameter. Usually an Order, User, or Product ID corresponding to a record in your database.
        /// </summary>
        public string Custom { get; set; }

        /// <summary>
        /// Set this to true to prevent the custom parameter from being viewed or modified after the button has been created. Use this if you are storing sensitive data in the custom parameter which you don�t want to be faked or leaked to the end user. Defaults to false. Note that if this value is set to true, the custom parameter will not be included in success or failure URLs, but it WILL be included in callbacks to your server.
        /// </summary>
        [JsonProperty("custom_secure")]
        public bool CustomSecure { get; set; }

        /// <summary>
        /// A custom callback URL specific to this button. It will receive the same information that would otherwise be sent to your Instant Payment Notification URL. If you have an Instant Payment Notification URL set on your account, this will be called instead � they will not both be called.
        /// </summary>
        [JsonProperty("callback_url")]
        public string CallbackUrl { get; set; }

        /// <summary>
        /// A custom success URL specific to this button. The user will be redirected to this URL after a successful payment. It will receive the same parameters that would otherwise be sent to the default success url set on the account.
        /// </summary>
        [JsonProperty("success_url")]
        public string SuccessUrl { get; set; }

        /// <summary>
        /// A custom cancel URL specific to this button. The user will be redirected to this URL after a canceled order. It will receive the same parameters that would otherwise be sent to the default cancel url set on the account.
        /// </summary>
        [JsonProperty("cancel_url")]
        public string CancelUrl { get; set; }

        /// <summary>
        /// Allow users to change the price on the generated button.
        /// </summary>
        [JsonProperty("variable_price")]
        public bool VariablePrice { get; set; }

        /// <summary>
        /// Show some suggested prices.
        /// </summary>
        [JsonProperty("choose_price")]
        public bool ChoosePrice { get; set; }

        /// <summary>
        /// A custom info URL specific to this button. Displayed to the user after a successful purchase for sharing. It will receive the same parameters that would otherwise be sent to the default info url set on the account.
        /// </summary>
        [JsonProperty("info_url")]
        public string InfoUrl { get; set; }

        /// <summary>
        /// Longer description of the item in case you want it added to the user�s transaction notes.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// One of buy_now_large, buy_now_small, donation_large, donation_small, subscription_large, subscription_small, custom_large, custom_small, and none. Default is buy_now_large. none is used if you plan on triggering the payment modal yourself using your own button or link.
        /// </summary>
        public ButtonStyle Style { get; set; }

        /// <summary>
        /// Allows you to customize the button text on custom_large and custom_small styles. Default is Pay With Bitcoin.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Collect email address from customer (not for use with inline iframes).
        /// </summary>
        [JsonProperty("include_email")]
        public bool IncludeEmail { get; set; }

        /// <summary>
        /// Collect shipping address from customer (not for use with inline iframes).
        /// </summary>
        [JsonProperty("include_address")]
        public bool IncludeAddress { get; set; }

        /// <summary>
        /// Auto-redirect users to success_url or cancel_url after payment. (cancel_url if the user pays the wrong amount.)
        /// </summary>
        [JsonProperty("auto_redirect")]
        public bool AutoRedirect { get; set; }
        
        /// <summary>
        /// Auto-redirect user to success_url after successful payment. (Overridden by auto_redirect if present)
        /// </summary>
        [JsonProperty("auto_redirect_success")]
        public bool AutoRedirectSuccess { get; set; }

        /// <summary>
        /// Auto-redirect user to cancel_url after payment of wrong amount. (Overridden by auto_redirect if present)
        /// </summary>
        [JsonProperty("auto_redirect_cancel")]
        public bool AutoRedirectCancel { get; set; }
    }
}