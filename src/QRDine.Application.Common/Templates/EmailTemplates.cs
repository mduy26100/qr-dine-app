namespace QRDine.Application.Common.Templates
{
    public static class EmailTemplates
    {
        public static string GetMerchantActivationTemplate(string firstName, string lastName, string merchantName, string verifyLink)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #2563eb; text-align: center;'>Chào mừng bạn đến với QRDine!</h2>
                    <p style='font-size: 16px; color: #333;'>Xin chào <strong>{firstName} {lastName}</strong>,</p>
                    <p style='font-size: 16px; color: #333;'>Cảm ơn bạn đã đăng ký mở cửa hàng <strong>{merchantName}</strong> trên nền tảng của chúng tôi. Để hoàn tất, vui lòng nhấn vào nút bên dưới để kích hoạt tài khoản:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{verifyLink}' style='background-color: #2563eb; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px;'>Kích Hoạt Tài Khoản</a>
                    </div>
                    <p style='font-size: 14px; color: #666;'><em>* Lưu ý: Link này chỉ có hiệu lực trong vòng 15 phút và chỉ sử dụng được 1 lần.</em></p>
                    <hr style='border: none; border-top: 1px solid #eaeaea; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #999; text-align: center;'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                </div>";
        }
    }
}
