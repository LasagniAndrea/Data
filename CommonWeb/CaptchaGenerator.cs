using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace EFS.Common.Web
{
    /// EG 20240524 [WI941][26663] Security : Account blocking - Captcha page
    /// EG 20240524 [WI941][26663] New Captcha generator
    public class CaptchaGenerator
    {
        /// <summary>
        /// Générateur de code CAPTCHA à la longueur spécifiée <paramref name="length"/>
        /// </summary>
        /// <param name="length">longueur du code demandé</param>
        /// <returns></returns>
        /// EG 20240524 [WI941][26663] New 
        public string GenerateCaptcha(int length = 6)
        {
            string captchaCode = GenerateRandomCode(length);
            return captchaCode;
        }
        /// <summary>
        /// Génération d'une image CAPTCHA sur la base d'un code spcéficié <paramref name="captchaCode"/>
        /// </summary>
        /// <param name="captchaCode">code</param>
        /// EG 20240524 [WI941][26663] New 
        public byte[] GenerateCaptchaImage(string captchaCode)
        {
            using (Bitmap bitmap = new Bitmap(340, 140))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.Clear(Color.White);

                    Font font = new Font("Arial", 20, FontStyle.Bold);
                    SizeF textSize = graphics.MeasureString(captchaCode, font);
                    LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                                         Color.Orange, Color.White, 1.2f, true);

                    graphics.DrawString(captchaCode, font, brush, 10, 10);

                    graphics.Clear(Color.White);
                    // Appliquer des transformations à l'image
                    ApplyTransformations(graphics, bitmap.Width, bitmap.Height, captchaCode);

                    // Sauvegarde de l'image en png
                    MemoryStream ms = new MemoryStream();
                    bitmap.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }
        /// <summary>
        /// Transformation (déformation) d'une image CAPTCHA construite sur la base d'un code <paramref name="captchaCode"/>
        /// </summary>
        /// <param name="graphics">Surface du dessin</param>
        /// <param name="width">longueur</param>
        /// <param name="height">hauteur</param>
        /// <param name="captchaCode">Code</param>
        /// EG 20240524 [WI941][26663] New 
        private void ApplyTransformations(Graphics graphics, int width, int height, string captchaCode)
        {
            Random random = new Random();

            // Créer une image temporaire pour dessiner le texte déformé
            using (Bitmap tempBitmap = new Bitmap(width, height))
            {
                using (Graphics tempGraphics = Graphics.FromImage(tempBitmap))
                {
                    tempGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                    tempGraphics.Clear(Color.Transparent);

                    // Dessiner le texte sur l'image temporaire
                    using (Font font = new Font("Arial", 20, FontStyle.Bold))
                    {
                        // Déterminer les dimensions du texte
                        SizeF textSize = tempGraphics.MeasureString(captchaCode, font);

                        // Générer des coordonnées aléatoires pour positionner le texte
                        int x = random.Next(Math.Max(0, width - (int)textSize.Width));
                        int y = random.Next(Math.Max(0, height - (int)textSize.Height));
                        PointF textPosition = new PointF(x, y);

                        // Appliquer une rotation aléatoire au texte
                        float angle = random.Next(-25, 15); // Angle de rotation aléatoire entre -15 et 15 degrés
                        tempGraphics.TranslateTransform(width / 2, height / 2);
                        tempGraphics.RotateTransform(angle);
                        tempGraphics.TranslateTransform(-width / 2, -height / 2);

                        // Dessiner le texte
                        tempGraphics.DrawString(captchaCode, font, Brushes.Black, textPosition);

                    }

                    // Effacer l'image principale
                    graphics.Clear(Color.Transparent);

                    // Dessiner l'image temporaire sur l'image principale
                    graphics.DrawImage(tempBitmap, new PointF(0, 0));
                }
            }
        }


        /// <summary>
        /// Retourne un code randomisé de longueur <paramref name="length"/>
        /// </summary>
        /// <param name="length">Longueur du code demandé</param>
        /// <returns></returns>
        /// EG 20240524 [WI941][26663] New 
        private string GenerateRandomCode(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@&#!$*_[]{}";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
