#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using FFImageLoading.Svg.Forms;
using Jdenticon;
using QRCoder;
using SwapWallet.Services;
using VicWeb;
using VicWeb.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Size = Xamarin.Forms.Size;


namespace SwapWallet.Resources
{

    public class ScreenSizeConverter : IValueConverter
    {
        private static double _screenHeight => DeviceDisplay.MainDisplayInfo.Height;
        private static double _screenWidth => DeviceDisplay.MainDisplayInfo.Width;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = parameter as string;

            if (param != null)
            {
                var split = param.Split(',');
                var width = double.Parse(split[0]);
                var height = double.Parse(split[1]);


                double w;
                double h;
                if (Device.RuntimePlatform == Device.UWP)
                {
                    var size = App.WindowSize;
                    var screenwidth = size.Width;
                    var screenheight = size.Height;
                    w = screenwidth * width;
                    h = screenheight * height;
                }
                else
                {
                    w = (_screenWidth * width) / DeviceDisplay.MainDisplayInfo.Density;
                    h = (_screenHeight * height) / DeviceDisplay.MainDisplayInfo.Density;
                }
                
                return new Size(w, h);
                //return $"'{w},{h}'";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ScreenHeightConverter : IValueConverter
    {
        private static double _screenHeight => DeviceDisplay.MainDisplayInfo.Height;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double param = System.Convert.ToDouble(parameter);

#if WINDOWS
if (BlackBoard.AppWindow != null)
{var screenheight = ((AppWindow)BlackBoard.AppWindow).Size.Height;
            return (screenheight * param);}
            return value;
#else
            return (_screenHeight * param) / DeviceDisplay.MainDisplayInfo.Density;
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ScreenWidthConverter : IValueConverter
    {
        private static double _screenWidth => DeviceDisplay.MainDisplayInfo.Width;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double param = (double)parameter;

#if WINDOWS
var screenwidth = ((AppWindow)BlackBoard.AppWindow).Size.Width;
            return (screenwidth * param);
#else
            return (_screenWidth * param) / DeviceDisplay.MainDisplayInfo.Density;
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LogoConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IExchange exchange)
                return $"Exchanges\\{exchange.Name}.png";
            if (value is MetaChain chain)
                return $"{chain.Name.Replace(" ", "").Replace("-", "_").ToLower()}.png";
            if (value is IToken token)
                if (!string.IsNullOrEmpty(token.LogoURI))
                    return token.LogoURI;
            return "help_icon.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class IdenticonConverter : IValueConverter
    {

        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var style = new IdenticonStyle
            {
                BackColor = Jdenticon.Rendering.Color.FromRgba(255, 255, 255, 0),
                ColorLightness = Range.Create(0.27f, 0.80f),
                GrayscaleLightness = Range.Create(0.27f, 0.75f),
                ColorSaturation = 0.72f,
                GrayscaleSaturation = 0.23f
            };

            var icon = Identicon.FromValue(value, 50);
            icon.Style = style;

            var source = SvgImageSource.FromSvgString(icon.ToSvg());


            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QRConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (!string.IsNullOrWhiteSpace(s))
            {
                ImageSource source;

                QRCodeData data;

                using (var g = new QRCodeGenerator())
                {
                    data = g.CreateQrCode(s, QRCodeGenerator.ECCLevel.Q);
                    PngByteQRCode png = new PngByteQRCode(data);
                    
                    source = ImageSource.FromStream(()=>new MemoryStream(png.GetGraphic(5)));
                }
                
                return source;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
