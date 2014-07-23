using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RWSelector.Helpers
{
    public static class UrlHelpers
    {

        private static string ColorImgFile(string colorCd, string colorName = null)
        {
            if (colorCd == "001") { return "black001.jpg"; }
            else if (colorCd == "002") { return "bone002.jpg"; }
            else if (colorCd == "003") { return "blue003.jpg"; }
            else if (colorCd == "004") { return "jade004.jpg"; }
            else if (colorCd == "005") { return "darkblue005.jpg"; }
            else if (colorCd == "006") { return "buckskin006.jpg"; }
            else if (colorCd == "007") { return "cordovan007.jpg"; }
            else if (colorCd == "008") { return "graphite008.jpg"; }
            else if (colorCd == "009") { return "sandstone009.jpg"; }
            else if (colorCd == "072") { return "lightgray072.jpg"; }
            else if (colorCd == "073") { return "beige073.jpg"; }
            else if (colorCd == "074") { return "mediumred074.jpg"; }
            else if (colorCd == "075") { return "brightred075.jpg"; }
            else if (colorCd == "076") { return "mist076.jpg"; }
            else if (colorCd == "077") { return "willow077.jpg"; }
            else if (colorCd == "078") { return "sahara078.jpg"; }
            else if (colorCd == "079") { return "darkcharcoal079.jpg"; }
            else if (colorCd == "080") { return "ice080.jpg"; }
            else if (colorCd == "081") { return "burgundy081.jpg"; }
            else if (colorCd == "082") { return "ecru082.jpg"; }
            else if (colorCd == "083") { return "titanium083.jpg"; }
            else if (colorCd == "084") { return "custard084.jpg"; }
            else if (colorCd == "085") { return "cafe085.jpg"; }
            else if (colorCd == "086") { return "fog086.jpg"; }
            else if (colorCd == "087") { return "ash087.jpg"; }
            else if (colorCd == "088") { return "earth088.jpg"; }
            else if (colorCd == "089") { return "slate089.jpg"; }
            else if (colorCd == "090") { return "fawn090.jpg"; }
            else if (colorCd == "091") { return "091_Dove_Grey.jpg"; }
            else if (colorCd == "092") { return "092_Stone.jpg"; }
            else if (colorCd == "093") { return "093_Shale.jpg"; }
            else if (colorCd == "094") { return "094_Desert.jpg"; }
            else if (colorCd == "095") { return "095_Bronze.jpg"; }
            else if (colorCd == "096") { return "096_Dark_Stone.jpg"; }
            else if (colorCd == "097") { return "097_Khaki.jpg"; }
            else if (colorCd == "098") { return "098_Parchment.jpg"; }
            else if (colorCd == "099") { return "099_Light_Bronze.jpg"; }
            else if (colorCd == "100") { return "100_Sand.jpg"; }
            else if (colorCd == "112") { return "gray112.jpg"; }
            else if (colorCd == "113") { return "brown113.jpg"; }
            else if (colorCd == "115") { return "doeskin115.jpg"; }
            else if (colorCd == "116") { return "silverleaf116.jpg"; }
            else if (colorCd == "117") { return "nickel117.jpg"; }
            else if (colorCd == "118") { return "olive118.jpg"; }
            else if (colorCd == "119") { return "camel119.jpg"; }
            else if (colorCd == "120") { return "darkbrown120.jpg"; }
            else if (colorCd == "121") { return "tan121.jpg"; }
            else if (colorCd == "122") { return "quicksilver122.jpg"; }
            else if (colorCd == "125") { return "ivory125.jpg"; }
            else if (colorCd == "139") { return "maroon139.jpg"; }
            else if (colorCd == "140") { return "opal140.jpg"; }
            else if (colorCd == "141") { return "snow141.jpg"; }
            else if (colorCd == "142") { return "saddle142.jpg"; }
            else if (colorCd == "517") { return "taupe517.jpg"; }
            else if (colorCd == "600") { return "red600.jpg"; }
            else if (colorCd == "780") { return "FlintSuede780.jpg"; }
            else if (colorCd == "781") { return "GraniteSuede781.jpg"; }
            else if (colorCd == "782") { return "MochaSuede782.jpg"; }
            else if (colorCd == "783") { return "MushroomSuede783.jpg"; }
            else if (colorCd == "784") { return "PewterSuede784.jpg"; }
            else if (colorCd == "799") { return "BlackSuede799.jpg"; }
            else if (colorCd == "601") { return "blue601.jpg"; }
            else if (colorCd == "603") { return "yellow603.jpg"; }
            else if (colorCd == "606") { return "canary606.jpg"; }
            else if (colorCd == "607") { return "silver607.jpg"; }
            else if (colorCd == "608") { return "pumpkin608.jpg"; }
            else if (colorCd == "609") { return "brick609.jpg"; }
            else if (colorCd == "402") { return "RanchBrown402.jpg"; }
            else if (colorCd == "124") { return "sage124.jpg"; }
            else if (colorCd == "101") { return "latte101.jpg"; }
            else
            {
                return ColorFileName(colorName.Trim().Replace(" ", string.Empty), colorCd.Trim(), "jpg");
            }
        }

        private static string ColorFileName(string colorName, string colorCd, string ext)
        {
            return string.Format("{0}{1}.{2}", colorName.Trim(), colorCd.Trim(), ext.Trim());
        }

        public static MvcHtmlString CstColorImgUrl(this UrlHelper helper, string colorCd, string colorName)
        {
            return MvcHtmlString.Create(string.Format("http://www.classicsofttrim.com/images/colors/{0}", ColorImgFile(colorCd, colorName)));
        }
    }
}