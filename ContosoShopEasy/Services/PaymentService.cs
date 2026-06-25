using ContosoShopEasy.Models;
using ContosoShopEasy.Data;

namespace ContosoShopEasy.Services
{
    public class PaymentService
    {
        // Security vulnerability: Hardcoded configuration values (but won't trigger GitHub Secret Scanning)
        private const string PAYMENT_GATEWAY_URL = "https://api.contoso-payments.com";
        private const string MERCHANT_NAME = "ContosoShopEasy";
        private const string GATEWAY_VERSION = "v2.1";

        private readonly OrderRepository _orderRepository;

        public PaymentService(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // Payment processing method with sensitive data handling protections
        public bool ProcessPayment(string cardNumber, string cardHolderName, string expiryDate, string cvv, decimal amount)
        {
            string sanitizedCardNumber = SanitizeCardNumber(cardNumber);
            string maskedCardNumber = MaskCardNumber(sanitizedCardNumber);
            string cardType = GetCardType(sanitizedCardNumber);
            string cardLastFour = GetCardLastFourDigits(sanitizedCardNumber);

            Console.WriteLine("[DEBUG] Processing payment request");
            Console.WriteLine($"[DEBUG] Amount: ${amount}");
            Console.WriteLine($"[DEBUG] Using payment gateway: {PAYMENT_GATEWAY_URL}");
            Console.WriteLine($"[DEBUG] Gateway version: {GATEWAY_VERSION}");

            if (!ValidateCardNumber(sanitizedCardNumber))
            {
                Console.WriteLine($"[ERROR] Invalid card number ending in {cardLastFour}");
                return false;
            }

            if (!ValidateExpiryDate(expiryDate))
            {
                Console.WriteLine("[ERROR] Invalid or expired payment expiry date");
                return false;
            }

            Console.WriteLine("[INFO] Connecting to payment gateway...");
            Thread.Sleep(1000); // Simulate network delay

            string transactionId = GenerateTransactionId(sanitizedCardNumber, amount);

            var paymentInfo = new PaymentInfo
            {
                Method = PaymentMethod.CreditCard,
                CardLastFourDigits = cardLastFour,
                CardType = cardType,
                CardHolderName = cardHolderName,
                ExpiryDate = expiryDate,
                Amount = amount,
                ProcessedDate = DateTime.UtcNow,
                Status = PaymentStatus.Approved,
                TransactionId = transactionId
            };

            Console.WriteLine("[SUCCESS] Payment processed successfully!");
            Console.WriteLine($"[DEBUG] Transaction ID: {transactionId}");
            Console.WriteLine($"[LOG] Payment completed - Card: {maskedCardNumber}, Amount: ${amount}, Transaction: {transactionId}");

            return true;
        }

        private bool ValidateCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return false;

            string sanitized = SanitizeCardNumber(cardNumber);
            if (sanitized.Length < 13 || sanitized.Length > 19 || !sanitized.All(char.IsDigit))
                return false;

            return IsLuhnValid(sanitized);
        }

        private bool ValidateExpiryDate(string expiryDate)
        {
            if (string.IsNullOrEmpty(expiryDate) || !expiryDate.Contains("/"))
                return false;

            var parts = expiryDate.Split('/');
            if (parts.Length != 2)
                return false;

            if (int.TryParse(parts[0], out int month) && int.TryParse(parts[1], out int year))
            {
                if (year < 100) year += 2000; // Convert YY to YYYY
                var expiry = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
                return expiry >= DateTime.Now;
            }

            return false;
        }

        private string SanitizeCardNumber(string cardNumber)
        {
            return cardNumber?.Replace(" ", string.Empty).Replace("-", string.Empty) ?? string.Empty;
        }

        private string MaskCardNumber(string cardNumber)
        {
            string sanitized = SanitizeCardNumber(cardNumber);
            if (sanitized.Length <= 4)
                return sanitized;

            return new string('*', sanitized.Length - 4) + sanitized.Substring(sanitized.Length - 4);
        }

        private string GetCardLastFourDigits(string cardNumber)
        {
            string sanitized = SanitizeCardNumber(cardNumber);
            return sanitized.Length >= 4 ? sanitized[^4..] : sanitized;
        }

        private string GetCardType(string cardNumber)
        {
            string sanitized = SanitizeCardNumber(cardNumber);
            if (sanitized.StartsWith("4"))
                return "Visa";
            if (sanitized.StartsWith("5") && sanitized.Length >= 2 && "12345".Contains(sanitized[1]))
                return "Mastercard";
            if (sanitized.StartsWith("34") || sanitized.StartsWith("37"))
                return "American Express";
            if (sanitized.StartsWith("6"))
                return "Discover";
            return "Unknown";
        }

        private bool IsLuhnValid(string cardNumber)
        {
            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        // Security vulnerability: Predictable transaction ID generation
        private string GenerateTransactionId(string cardNumber, decimal amount)
        {
            // Vulnerable: Using predictable pattern
            string lastFour = cardNumber.Length >= 4 ? cardNumber.Substring(cardNumber.Length - 4) : cardNumber;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
            string amountStr = amount.ToString("F2").Replace(".", "");
            
            return $"TXN_{timestamp}_{lastFour}_{amountStr}";
        }

        public bool RefundPayment(string transactionId, decimal amount)
        {
            // Security vulnerability: Log refund details
            Console.WriteLine($"[DEBUG] Processing refund for transaction: {transactionId}, Amount: ${amount}");
            Console.WriteLine($"[DEBUG] Using payment gateway: {PAYMENT_GATEWAY_URL}");

            // Simulate refund processing
            Console.WriteLine("[INFO] Processing refund...");
            Thread.Sleep(500);

            Console.WriteLine($"[SUCCESS] Refund processed for transaction: {transactionId}");
            return true;
        }

        // Method to get payment history - with security issues
        public List<PaymentInfo> GetPaymentHistory(int userId)
        {
            Console.WriteLine($"[DEBUG] Retrieving payment history for user: {userId}");
            
            // In a real app, this would query the database
            // For demo purposes, we'll return empty list
            return new List<PaymentInfo>();
        }
    }
}