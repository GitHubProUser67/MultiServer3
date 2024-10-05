using ComponentAce.Compression.Libs.zlib;
using ZstdSharp;
using ZstdSharp.Unsafe;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using NetworkLibrary.Extension;
using Ionic.Exploration;
using System.Threading;
using System.Buffers;
using HashLib;

namespace NetworkLibrary.HTTP
{
    public partial class HTTPProcessor
    {
        private static object _StackLock = new object();

        public static readonly Dictionary<string, string> _mimeTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
             #region Big freaking list of mime types

            // combination of values from Windows 7 Registry and 
            // from C:\Windows\System32\inetsrv\config\applicationHost.config
            // some added, including .7z , .dat, .pup and .mkv
            {".323", "text/h323"},
            {".3g2", "video/3gpp2"},
            {".3gp", "video/3gpp"},
            {".3gp2", "video/3gpp2"},
            {".3gpp", "video/3gpp"},
            {".7z", "application/x-7z-compressed"},
            {".aa", "audio/audible"},
            {".AAC", "audio/aac"},
            {".aaf", "application/octet-stream"},
            {".aax", "audio/vnd.audible.aax"},
            {".ac3", "audio/ac3"},
            {".aca", "application/octet-stream"},
            {".accda", "application/msaccess.addin"},
            {".accdb", "application/msaccess"},
            {".accdc", "application/msaccess.cab"},
            {".accde", "application/msaccess"},
            {".accdr", "application/msaccess.runtime"},
            {".accdt", "application/msaccess"},
            {".accdw", "application/msaccess.webapplication"},
            {".accft", "application/msaccess.ftemplate"},
            {".acx", "application/internet-property-stream"},
            {".AddIn", "text/xml"},
            {".ade", "application/msaccess"},
            {".adobebridge", "application/x-bridge-url"},
            {".adp", "application/msaccess"},
            {".ADT", "audio/vnd.dlna.adts"},
            {".ADTS", "audio/aac"},
            {".afm", "application/octet-stream"},
            {".ai", "application/postscript"},
            {".aif", "audio/x-aiff"},
            {".aifc", "audio/aiff"},
            {".aiff", "audio/aiff"},
            {".air", "application/vnd.adobe.air-application-installer-package+zip"},
            {".amc", "application/x-mpeg"},
            {".application", "application/x-ms-application"},
            {".art", "image/x-jg"},
            {".asa", "application/xml"},
            {".asax", "application/xml"},
            {".ascx", "application/xml"},
            {".asd", "application/octet-stream"},
            {".asf", "video/x-ms-asf"},
            {".ashx", "application/xml"},
            {".asi", "application/octet-stream"},
            {".asm", "text/plain"},
            {".asmx", "application/xml"},
            {".aspx", "application/xml"},
            {".asr", "video/x-ms-asf"},
            {".asx", "video/x-ms-asf"},
            {".atom", "application/atom+xml"},
            {".au", "audio/basic"},
            {".avi", "video/x-msvideo"},
            {".axs", "application/olescript"},
            {".bas", "text/plain"},
            {".bcpio", "application/x-bcpio"},
            {".bin", "application/octet-stream"},
            {".bmp", "image/bmp"},
            {".c", "text/plain"},
            {".cab", "application/octet-stream"},
            {".caf", "audio/x-caf"},
            {".calx", "application/vnd.ms-office.calx"},
            {".cat", "application/vnd.ms-pki.seccat"},
            {".cc", "text/plain"},
            {".cd", "text/plain"},
            {".cdda", "audio/aiff"},
            {".cdf", "application/x-cdf"},
            {".cer", "application/x-x509-ca-cert"},
            {".chm", "application/octet-stream"},
            {".class", "application/x-java-applet"},
            {".clp", "application/x-msclip"},
            {".cmx", "image/x-cmx"},
            {".cnf", "text/plain"},
            {".cod", "image/cis-cod"},
            {".config", "application/xml"},
            {".contact", "text/x-ms-contact"},
            {".coverage", "application/xml"},
            {".cpio", "application/x-cpio"},
            {".cpp", "text/plain"},
            {".crd", "application/x-mscardfile"},
            {".crl", "application/pkix-crl"},
            {".crt", "application/x-x509-ca-cert"},
            {".cs", "text/plain"},
            {".csdproj", "text/plain"},
            {".csh", "application/x-csh"},
            {".csproj", "text/plain"},
            {".css", "text/css"},
            {".csv", "text/csv"},
            {".cur", "application/octet-stream"},
            {".cxx", "text/plain"},
            {".dat", "application/octet-stream"},
            {".datasource", "application/xml"},
            {".dbproj", "text/plain"},
            {".dcr", "application/x-director"},
            {".def", "text/plain"},
            {".deploy", "application/octet-stream"},
            {".der", "application/x-x509-ca-cert"},
            {".dgml", "application/xml"},
            {".dib", "image/bmp"},
            {".dif", "video/x-dv"},
            {".dir", "application/x-director"},
            {".disco", "text/xml"},
            {".dll", "application/x-msdownload"},
            {".dll.config", "text/xml"},
            {".dlm", "text/dlm"},
            {".doc", "application/msword"},
            {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".dot", "application/msword"},
            {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
            {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
            {".dsp", "application/octet-stream"},
            {".dsw", "text/plain"},
            {".dtd", "text/xml"},
            {".dtsConfig", "text/xml"},
            {".dv", "video/x-dv"},
            {".dvi", "application/x-dvi"},
            {".dwf", "drawing/x-dwf"},
            {".dwp", "application/octet-stream"},
            {".dxr", "application/x-director"},
            {".eml", "message/rfc822"},
            {".emz", "application/octet-stream"},
            {".eot", "application/octet-stream"},
            {".eps", "application/postscript"},
            {".etl", "application/etl"},
            {".etx", "text/x-setext"},
            {".evy", "application/envoy"},
            {".exe", "application/octet-stream"},
            {".exe.config", "text/xml"},
            {".fdf", "application/vnd.fdf"},
            {".fif", "application/fractals"},
            {".filters", "Application/xml"},
            {".fla", "application/octet-stream"},
            {".flr", "x-world/x-vrml"},
            {".flv", "video/x-flv"},
            {".fsscript", "application/fsharp-script"},
            {".fsx", "application/fsharp-script"},
            {".generictest", "application/xml"},
            {".gif", "image/gif"},
            {".group", "text/x-ms-group"},
            {".gsm", "audio/x-gsm"},
            {".gtar", "application/x-gtar"},
            {".gz", "application/x-gzip"},
            {".h", "text/plain"},
            {".hdf", "application/x-hdf"},
            {".hdml", "text/x-hdml"},
            {".hhc", "application/x-oleobject"},
            {".hhk", "application/octet-stream"},
            {".hhp", "application/octet-stream"},
            {".hlp", "application/winhlp"},
            {".hpp", "text/plain"},
            {".hqx", "application/mac-binhex40"},
            {".hta", "application/hta"},
            {".htc", "text/x-component"},
            {".htm", "text/html"},
            {".html", "text/html"},
            {".htt", "text/webviewhtml"},
            {".hxa", "application/xml"},
            {".hxc", "application/xml"},
            {".hxd", "application/octet-stream"},
            {".hxe", "application/xml"},
            {".hxf", "application/xml"},
            {".hxh", "application/octet-stream"},
            {".hxi", "application/octet-stream"},
            {".hxk", "application/xml"},
            {".hxq", "application/octet-stream"},
            {".hxr", "application/octet-stream"},
            {".hxs", "application/octet-stream"},
            {".hxt", "text/html"},
            {".hxv", "application/xml"},
            {".hxw", "application/octet-stream"},
            {".hxx", "text/plain"},
            {".i", "text/plain"},
            {".ico", "image/x-icon"},
            {".ics", "application/octet-stream"},
            {".idl", "text/plain"},
            {".ief", "image/ief"},
            {".iii", "application/x-iphone"},
            {".inc", "text/plain"},
            {".inf", "application/octet-stream"},
            {".inl", "text/plain"},
            {".ins", "application/x-internet-signup"},
            {".ipa", "application/x-itunes-ipa"},
            {".ipg", "application/x-itunes-ipg"},
            {".ipproj", "text/plain"},
            {".ipsw", "application/x-itunes-ipsw"},
            {".iqy", "text/x-ms-iqy"},
            {".isp", "application/x-internet-signup"},
            {".ite", "application/x-itunes-ite"},
            {".itlp", "application/x-itunes-itlp"},
            {".itms", "application/x-itunes-itms"},
            {".itpc", "application/x-itunes-itpc"},
            {".IVF", "video/x-ivf"},
            {".jar", "application/java-archive"},
            {".java", "application/octet-stream"},
            {".jck", "application/liquidmotion"},
            {".jcz", "application/liquidmotion"},
            {".jfif", "image/pjpeg"},
            {".jnlp", "application/x-java-jnlp-file"},
            {".jpb", "application/octet-stream"},
            {".jpe", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "application/x-javascript"},
            {".json", "application/json"},
            {".jsx", "text/jscript"},
            {".jsxbin", "text/plain"},
            {".latex", "application/x-latex"},
            {".library-ms", "application/windows-library+xml"},
            {".lit", "application/x-ms-reader"},
            {".loadtest", "application/xml"},
            {".lpk", "application/octet-stream"},
            {".lsf", "video/x-la-asf"},
            {".lst", "text/plain"},
            {".lsx", "video/x-la-asf"},
            {".lzh", "application/octet-stream"},
            {".m13", "application/x-msmediaview"},
            {".m14", "application/x-msmediaview"},
            {".m1v", "video/mpeg"},
            {".m2t", "video/vnd.dlna.mpeg-tts"},
            {".m2ts", "video/vnd.dlna.mpeg-tts"},
            {".m2v", "video/mpeg"},
            {".m3u", "audio/x-mpegurl"},
            {".m3u8", "audio/x-mpegurl"},
            {".m4a", "audio/m4a"},
            {".m4b", "audio/m4b"},
            {".m4p", "audio/m4p"},
            {".m4r", "audio/x-m4r"},
            {".m4v", "video/x-m4v"},
            {".mac", "image/x-macpaint"},
            {".mak", "text/plain"},
            {".man", "application/x-troff-man"},
            {".manifest", "application/x-ms-manifest"},
            {".map", "text/plain"},
            {".master", "application/xml"},
            {".mda", "application/msaccess"},
            {".mdb", "application/x-msaccess"},
            {".mde", "application/msaccess"},
            {".mdp", "application/octet-stream"},
            {".me", "application/x-troff-me"},
            {".mfp", "application/x-shockwave-flash"},
            {".mht", "message/rfc822"},
            {".mhtml", "message/rfc822"},
            {".mid", "audio/mid"},
            {".midi", "audio/mid"},
            {".mix", "application/octet-stream"},
            {".mk", "text/plain"},
            {".mkv", "video/x-matroska"},
            {".mmf", "application/x-smaf"},
            {".mno", "text/xml"},
            {".mny", "application/x-msmoney"},
            {".mod", "video/mpeg"},
            {".mov", "video/quicktime"},
            {".movie", "video/x-sgi-movie"},
            {".mp2", "video/mpeg"},
            {".mp2v", "video/mpeg"},
            {".mp3", "audio/mpeg"},
            {".mp4", "video/mp4"},
            {".mp4v", "video/mp4"},
            {".mpa", "video/mpeg"},
            {".mpe", "video/mpeg"},
            {".mpeg", "video/mpeg"},
            {".mpf", "application/vnd.ms-mediapackage"},
            {".mpg", "video/mpeg"},
            {".mpp", "application/vnd.ms-project"},
            {".mpv2", "video/mpeg"},
            {".mqv", "video/quicktime"},
            {".ms", "application/x-troff-ms"},
            {".msi", "application/octet-stream"},
            {".mso", "application/octet-stream"},
            {".mts", "video/vnd.dlna.mpeg-tts"},
            {".mtx", "application/xml"},
            {".mvb", "application/x-msmediaview"},
            {".mvc", "application/x-miva-compiled"},
            {".mxp", "application/x-mmxp"},
            {".nc", "application/x-netcdf"},
            {".nsc", "video/x-ms-asf"},
            {".nws", "message/rfc822"},
            {".ocx", "application/octet-stream"},
            {".oda", "application/oda"},
            {".odc", "text/x-ms-odc"},
            {".odh", "text/plain"},
            {".odl", "text/plain"},
            {".odp", "application/vnd.oasis.opendocument.presentation"},
            {".ods", "application/oleobject"},
            {".odt", "application/vnd.oasis.opendocument.text"},
            {".one", "application/onenote"},
            {".onea", "application/onenote"},
            {".onepkg", "application/onenote"},
            {".onetmp", "application/onenote"},
            {".onetoc", "application/onenote"},
            {".onetoc2", "application/onenote"},
            {".orderedtest", "application/xml"},
            {".osdx", "application/opensearchdescription+xml"},
            {".p10", "application/pkcs10"},
            {".p12", "application/x-pkcs12"},
            {".p7b", "application/x-pkcs7-certificates"},
            {".p7c", "application/pkcs7-mime"},
            {".p7m", "application/pkcs7-mime"},
            {".p7r", "application/x-pkcs7-certreqresp"},
            {".p7s", "application/pkcs7-signature"},
            {".pbm", "image/x-portable-bitmap"},
            {".pcast", "application/x-podcast"},
            {".pct", "image/pict"},
            {".pcx", "application/octet-stream"},
            {".pcz", "application/octet-stream"},
            {".pdf", "application/pdf"},
            {".pfb", "application/octet-stream"},
            {".pfm", "application/octet-stream"},
            {".pfx", "application/x-pkcs12"},
            {".pgm", "image/x-portable-graymap"},
            {".pic", "image/pict"},
            {".pict", "image/pict"},
            {".pkgdef", "text/plain"},
            {".pkgundef", "text/plain"},
            {".pko", "application/vnd.ms-pki.pko"},
            {".pls", "audio/scpls"},
            {".pma", "application/x-perfmon"},
            {".pmc", "application/x-perfmon"},
            {".pml", "application/x-perfmon"},
            {".pmr", "application/x-perfmon"},
            {".pmw", "application/x-perfmon"},
            {".png", "image/png"},
            {".pnm", "image/x-portable-anymap"},
            {".pnt", "image/x-macpaint"},
            {".pntg", "image/x-macpaint"},
            {".pnz", "image/png"},
            {".pot", "application/vnd.ms-powerpoint"},
            {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
            {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
            {".ppa", "application/vnd.ms-powerpoint"},
            {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
            {".ppm", "image/x-portable-pixmap"},
            {".pps", "application/vnd.ms-powerpoint"},
            {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
            {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
            {".ppt", "application/vnd.ms-powerpoint"},
            {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {".prf", "application/pics-rules"},
            {".prm", "application/octet-stream"},
            {".prx", "application/octet-stream"},
            {".ps", "application/postscript"},
            {".psc1", "application/PowerShell"},
            {".psd", "application/octet-stream"},
            {".psess", "application/xml"},
            {".psm", "application/octet-stream"},
            {".psp", "application/octet-stream"},
            {".pub", "application/x-mspublisher"},
            {".pup", "application/x-ps3-update"},
            {".pwz", "application/vnd.ms-powerpoint"},
            {".qht", "text/x-html-insertion"},
            {".qhtm", "text/x-html-insertion"},
            {".qt", "video/quicktime"},
            {".qti", "image/x-quicktime"},
            {".qtif", "image/x-quicktime"},
            {".qtl", "application/x-quicktimeplayer"},
            {".qxd", "application/octet-stream"},
            {".ra", "audio/x-pn-realaudio"},
            {".ram", "audio/x-pn-realaudio"},
            {".rar", "application/octet-stream"},
            {".ras", "image/x-cmu-raster"},
            {".rat", "application/rat-file"},
            {".rc", "text/plain"},
            {".rc2", "text/plain"},
            {".rct", "text/plain"},
            {".rdlc", "application/xml"},
            {".resx", "application/xml"},
            {".rf", "image/vnd.rn-realflash"},
            {".rgb", "image/x-rgb"},
            {".rgs", "text/plain"},
            {".rm", "application/vnd.rn-realmedia"},
            {".rmi", "audio/mid"},
            {".rmp", "application/vnd.rn-rn_music_package"},
            {".roff", "application/x-troff"},
            {".rpm", "audio/x-pn-realaudio-plugin"},
            {".rqy", "text/x-ms-rqy"},
            {".rtf", "application/rtf"},
            {".rtx", "text/richtext"},
            {".ruleset", "application/xml"},
            {".s", "text/plain"},
            {".safariextz", "application/x-safari-safariextz"},
            {".scd", "application/x-msschedule"},
            {".sct", "text/scriptlet"},
            {".sd2", "audio/x-sd2"},
            {".sdp", "application/sdp"},
            {".sea", "application/octet-stream"},
            {".searchConnector-ms", "application/windows-search-connector+xml"},
            {".setpay", "application/set-payment-initiation"},
            {".setreg", "application/set-registration-initiation"},
            {".settings", "application/xml"},
            {".sgimb", "application/x-sgimb"},
            {".sgml", "text/sgml"},
            {".sh", "application/x-sh"},
            {".shar", "application/x-shar"},
            {".shtml", "text/html"},
            {".sit", "application/x-stuffit"},
            {".sitemap", "application/xml"},
            {".skin", "application/xml"},
            {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
            {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
            {".slk", "application/vnd.ms-excel"},
            {".sln", "text/plain"},
            {".slupkg-ms", "application/x-ms-license"},
            {".smd", "audio/x-smd"},
            {".smi", "application/octet-stream"},
            {".smx", "audio/x-smd"},
            {".smz", "audio/x-smd"},
            {".snd", "audio/basic"},
            {".snippet", "application/xml"},
            {".snp", "application/octet-stream"},
            {".sol", "text/plain"},
            {".sor", "text/plain"},
            {".spc", "application/x-pkcs7-certificates"},
            {".spl", "application/futuresplash"},
            {".src", "application/x-wais-source"},
            {".srf", "text/plain"},
            {".SSISDeploymentManifest", "text/xml"},
            {".ssm", "application/streamingmedia"},
            {".sst", "application/vnd.ms-pki.certstore"},
            {".stl", "application/vnd.ms-pki.stl"},
            {".sv4cpio", "application/x-sv4cpio"},
            {".sv4crc", "application/x-sv4crc"},
            {".svc", "application/xml"},
            {".swf", "application/x-shockwave-flash"},
            {".t", "application/x-troff"},
            {".tar", "application/x-tar"},
            {".tcl", "application/x-tcl"},
            {".testrunconfig", "application/xml"},
            {".testsettings", "application/xml"},
            {".tex", "application/x-tex"},
            {".texi", "application/x-texinfo"},
            {".texinfo", "application/x-texinfo"},
            {".tgz", "application/x-compressed"},
            {".thmx", "application/vnd.ms-officetheme"},
            {".thn", "application/octet-stream"},
            {".tif", "image/tiff"},
            {".tiff", "image/tiff"},
            {".tlh", "text/plain"},
            {".tli", "text/plain"},
            {".toc", "application/octet-stream"},
            {".tr", "application/x-troff"},
            {".trm", "application/x-msterminal"},
            {".trx", "application/xml"},
            {".ts", "video/vnd.dlna.mpeg-tts"},
            {".tsv", "text/tab-separated-values"},
            {".ttf", "application/octet-stream"},
            {".tts", "video/vnd.dlna.mpeg-tts"},
            {".txt", "text/plain"},
            {".u32", "application/octet-stream"},
            {".uls", "text/iuls"},
            {".user", "text/plain"},
            {".ustar", "application/x-ustar"},
            {".vb", "text/plain"},
            {".vbdproj", "text/plain"},
            {".vbk", "video/mpeg"},
            {".vbproj", "text/plain"},
            {".vbs", "text/vbscript"},
            {".vcf", "text/x-vcard"},
            {".vcproj", "Application/xml"},
            {".vcs", "text/plain"},
            {".vcxproj", "Application/xml"},
            {".vddproj", "text/plain"},
            {".vdp", "text/plain"},
            {".vdproj", "text/plain"},
            {".vdx", "application/vnd.ms-visio.viewer"},
            {".vml", "text/xml"},
            {".vscontent", "application/xml"},
            {".vsct", "text/xml"},
            {".vsd", "application/vnd.visio"},
            {".vsi", "application/ms-vsi"},
            {".vsix", "application/vsix"},
            {".vsixlangpack", "text/xml"},
            {".vsixmanifest", "text/xml"},
            {".vsmdi", "application/xml"},
            {".vspscc", "text/plain"},
            {".vss", "application/vnd.visio"},
            {".vsscc", "text/plain"},
            {".vssettings", "text/xml"},
            {".vssscc", "text/plain"},
            {".vst", "application/vnd.visio"},
            {".vstemplate", "text/xml"},
            {".vsto", "application/x-ms-vsto"},
            {".vsw", "application/vnd.visio"},
            {".vsx", "application/vnd.visio"},
            {".vtx", "application/vnd.visio"},
            {".wav", "audio/wav"},
            {".wave", "audio/wav"},
            {".wax", "audio/x-ms-wax"},
            {".wbk", "application/msword"},
            {".wbmp", "image/vnd.wap.wbmp"},
            {".wcm", "application/vnd.ms-works"},
            {".wdb", "application/vnd.ms-works"},
            {".wdp", "image/vnd.ms-photo"},
            {".webarchive", "application/x-safari-webarchive"},
            {".webtest", "application/xml"},
            {".wiq", "application/xml"},
            {".wiz", "application/msword"},
            {".wks", "application/vnd.ms-works"},
            {".WLMP", "application/wlmoviemaker"},
            {".wlpginstall", "application/x-wlpg-detect"},
            {".wlpginstall3", "application/x-wlpg3-detect"},
            {".wm", "video/x-ms-wm"},
            {".wma", "audio/x-ms-wma"},
            {".wmd", "application/x-ms-wmd"},
            {".wmf", "application/x-msmetafile"},
            {".wml", "text/vnd.wap.wml"},
            {".wmlc", "application/vnd.wap.wmlc"},
            {".wmls", "text/vnd.wap.wmlscript"},
            {".wmlsc", "application/vnd.wap.wmlscriptc"},
            {".wmp", "video/x-ms-wmp"},
            {".wmv", "video/x-ms-wmv"},
            {".wmx", "video/x-ms-wmx"},
            {".wmz", "application/x-ms-wmz"},
            {".wpl", "application/vnd.ms-wpl"},
            {".wps", "application/vnd.ms-works"},
            {".wri", "application/x-mswrite"},
            {".wrl", "x-world/x-vrml"},
            {".wrz", "x-world/x-vrml"},
            {".wsc", "text/scriptlet"},
            {".wsdl", "text/xml"},
            {".wvx", "video/x-ms-wvx"},
            {".x", "application/directx"},
            {".xaf", "x-world/x-vrml"},
            {".xaml", "application/xaml+xml"},
            {".xap", "application/x-silverlight-app"},
            {".xbap", "application/x-ms-xbap"},
            {".xbm", "image/x-xbitmap"},
            {".xdr", "text/plain"},
            {".xht", "application/xhtml+xml"},
            {".xhtml", "application/xhtml+xml"},
            {".xla", "application/vnd.ms-excel"},
            {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
            {".xlc", "application/vnd.ms-excel"},
            {".xld", "application/vnd.ms-excel"},
            {".xlk", "application/vnd.ms-excel"},
            {".xll", "application/vnd.ms-excel"},
            {".xlm", "application/vnd.ms-excel"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
            {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".xlt", "application/vnd.ms-excel"},
            {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
            {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
            {".xlw", "application/vnd.ms-excel"},
            {".xml", "text/xml"},
            {".xmta", "application/xml"},
            {".xof", "x-world/x-vrml"},
            {".XOML", "text/plain"},
            {".xpm", "image/x-xpixmap"},
            {".xps", "application/vnd.ms-xpsdocument"},
            {".xrm-ms", "text/xml"},
            {".xsc", "application/xml"},
            {".xsd", "text/xml"},
            {".xsf", "text/xml"},
            {".xsl", "text/xml"},
            {".xslt", "text/xml"},
            {".xsn", "application/octet-stream"},
            {".xss", "application/xml"},
            {".xtp", "application/octet-stream"},
            {".xwd", "image/x-xwindowdump"},
            {".z", "application/x-compress"},
            {".zip", "application/x-zip-compressed"},
            #endregion
        };

        public static readonly Dictionary<string, byte[]> _PathernDictionary = new Dictionary<string, byte[]>()
        {
            // Add more entries as needed
            { "text/html", new byte[] { 0x3C, 0x21, 0x44, 0x4F, 0x43, 0x54, 0x59, 0x50, 0x45, 0x20 } },
            { "video/mp4", new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70 } }
        };

        public static string[] _DefaultFiles =
        {
            "index.html",
            "index.htm",
            "default.html",
            "default.htm",
            "home.html",
            "home.htm",
            "home.cgi",
            "welcome.html",
            "welcome.htm",
            "index.php",
            "default.aspx",
            "default.asp"
        };

        public static string DecodeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        // HTTP requires that responses contain the proper MIME type. This quick mapping list below
        // contains many more mimetypes than System.Web.MimeMapping

        // http://stackoverflow.com/questions/1029740/get-mime-type-from-filename-extension

        public static string GetMimeType(string extension, Dictionary<string, string> mimeTypesDic)
        {
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";
            else
            {
                if (!extension.StartsWith("."))
                    extension = "." + extension;

                return mimeTypesDic.TryGetValue(extension, out string mime) ? mime : "application/octet-stream";
            }
        }

        public static bool CheckHeaderMatch(byte[] byteArray, int startIndex, string header)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (startIndex + i >= byteArray.Length || byteArray[startIndex + i] != (byte)header[i])
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Check if it's need to return 304 instead.
        /// </summary>
        /// <param name="Since">Value of Since based request HTTP header.</param>
        /// <param name="filePath">Path of the file to check on.</param>
        /// <param name="reverseConditional">(Optional) use reversed result.</param>
        /// <returns><c>true</c> if need to return 304 or <c>false</c> if need to return 200 or 404</returns>
        public static bool CheckLastWriteTime(string filePath, string Since, bool reverseConditional = false)
        {
            if (string.IsNullOrWhiteSpace(Since) || string.IsNullOrEmpty(filePath)) return false;

            try
            {
                DateTimeOffset? time = ToDateTimeOffset(Since);

                if (time.HasValue && File.Exists(filePath))
                    return reverseConditional ? time.Value < new FileInfo(filePath).LastWriteTime : time.Value >= new FileInfo(filePath).LastWriteTime;
            }
            catch
            {

            }

            return false;
        }

        public static string ExtractBoundary(string contentType)
        {
            if (!string.IsNullOrEmpty(contentType))
            {
                int boundaryIndex = contentType.IndexOf("boundary=", StringComparison.InvariantCultureIgnoreCase);
                if (boundaryIndex != -1)
                    return contentType.Substring(boundaryIndex + 9);
            }

            return contentType ?? string.Empty;
        }

        public static string ExtractDirtyProxyPath(string referer)
        {
            if (string.IsNullOrEmpty(referer))
                return string.Empty;
#if NET7_0_OR_GREATER
            // Match the input string with the pattern
            Match match = DirtyProxyRegex().Match(referer);
#else
            Match match = new Regex(@"^(.*?http://.*?http://)([^/]+)(.*)$").Match(referer);
#endif

            // Check if the pattern is matched
            if (match.Success)
                // Extract the absolute path from the later URL
                return match.Groups[3].Value;

            return string.Empty;
        }

        public static Dictionary<string, string> ExtractAndSortUrlEncodedPOSTData(byte[] urlEncodedDataByte)
        {
            // Use HttpUtility.ParseQueryString to parse the URL-encoded data
            NameValueCollection formData = HttpUtility.ParseQueryString(Encoding.UTF8.GetString(urlEncodedDataByte));

            // Convert the NameValueCollection to a dictionary for easy sorting
            Dictionary<string, string> formDataDictionary = new Dictionary<string, string>();
            foreach (string key in formData.AllKeys)
            {
                if (key != null)
                    formDataDictionary[key] = formData[key] ?? string.Empty;
            }
#if NET5_0_OR_GREATER
            return new Dictionary<string, string>(formDataDictionary.OrderBy(x => x.Key));
#else
            return new Dictionary<string, string>(formDataDictionary.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value));
#endif
        }

        public static string RemoveQueryString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            int indexOfQuestionMark = input.IndexOf('?');

            if (indexOfQuestionMark >= 0)
                return input.Substring(0, indexOfQuestionMark);
            else
                return input;
        }

        public static string RemoveDiacritics(string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }


        /// <summary>
        /// Convert string to DateTimeOffset
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>DateTimeOffset</returns>
        /// <exception cref="InvalidCastException">Throws if the <paramref name="input"/> is not understood by .NET Runtime.</exception>
        public static DateTimeOffset? ToDateTimeOffset(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            // see for docs: https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset.parse?view=net-6.0
            return input.ToLower() == "now" ? DateTimeOffset.Now : DateTimeOffset.Parse(input);
        }

        public static byte[] CompressZstd(byte[] input)
        {
            using (Compressor compressor = new Compressor())
                return compressor.Wrap(input).ToArray();
        }
#if NET5_0_OR_GREATER
        public static byte[] CompressBrotli(byte[] input)
        {
            using MemoryStream output = new MemoryStream();
            using (BrotliStream brStream = new BrotliStream(output, CompressionLevel.Fastest))
            {
                brStream.Write(input, 0, input.Length);
                brStream.Flush();
            }

            return output.ToArray();
        }
#endif
        public static byte[] CompressGzip(byte[] input)
        {
            using (MemoryStream output = new MemoryStream())
            using (GZipStream gzipStream = new GZipStream(output, CompressionLevel.Fastest))
            {
                gzipStream.Write(input, 0, input.Length);
                gzipStream.Close();
                return output.ToArray();
            }
        }

        public static byte[] Inflate(byte[] input)
        {
            using (MemoryStream output = new MemoryStream())
            using (ZOutputStream zlibStream = new ZOutputStream(output, 1, true))
            {
                zlibStream.Write(input, 0, input.Length);
                zlibStream.finish();
                return output.ToArray();
            }
        }

        public static Stream ZstdCompressStream(Stream input)
        {
            Stream outMemoryStream;
            if (input.Length > 2147483648)
                outMemoryStream = new HugeMemoryStream();
            else
                outMemoryStream = new MemoryStream();
            using (CompressionStream outZStream = new CompressionStream(outMemoryStream))
            {
                outZStream.SetParameter(ZSTD_cParameter.ZSTD_c_nbWorkers, 2);
                CopyStream(input, outZStream, 4096);
            }
            input.Close();
            input.Dispose();
            outMemoryStream.Seek(0, SeekOrigin.Begin);
            return outMemoryStream;
        }
#if NET5_0_OR_GREATER
        public static Stream BrotliCompressStream(Stream input)
        {
            Stream outMemoryStream;
            if (input.Length > 2147483648)
                outMemoryStream = new HugeMemoryStream();
            else
                outMemoryStream = new MemoryStream();
            BrotliStream outBStream = new BrotliStream(outMemoryStream, CompressionLevel.Fastest);
            CopyStream(input, outBStream, 4096);
            input.Close();
            input.Dispose();
            outMemoryStream.Seek(0, SeekOrigin.Begin);
            return outMemoryStream;
        }
#endif
        public static Stream GzipCompressStream(Stream input)
        {
            Stream outMemoryStream;
            if (input.Length > 2147483648)
                outMemoryStream = new HugeMemoryStream();
            else
                outMemoryStream = new MemoryStream();
            using (ParallelGZipOutputStream outGStream = new ParallelGZipOutputStream(outMemoryStream, Ionic.Zlib.CompressionLevel.BestSpeed, Ionic.Zlib.CompressionStrategy.Filtered, true, 2))
                CopyStream(input, outGStream, 4096, false);
            input.Close();
            input.Dispose();
            outMemoryStream.Seek(0, SeekOrigin.Begin);
            return outMemoryStream;
        }

        public static Stream InflateStream(Stream input)
        {
            Stream outMemoryStream;
            if (input.Length > 2147483648)
                outMemoryStream = new HugeMemoryStream();
            else
                outMemoryStream = new MemoryStream();
            ZOutputStream outZStream = new ZOutputStream(outMemoryStream, 1, true);
            CopyStream(input, outZStream, 4096, false);
            outZStream.finish();
            input.Close();
            input.Dispose();
            outMemoryStream.Seek(0, SeekOrigin.Begin);
            return outMemoryStream;
        }

        /// <summary>
        /// Compute the MD5 checksum of a stream.
        /// <para>Calcul la somme des contr�les en MD5 d'un stream.</para>
        /// </summary>
        /// <param name="input">The input stream (must be seekable).</param>
        /// <returns>A string.</returns>
        public static string ComputeStreamMD5(Stream input)
        {
            if (!input.CanSeek)
                return null;

            string md5Hash = NetHasher.ComputeMD5String(input).ToLower();
            input.Seek(0, SeekOrigin.Begin);
            return md5Hash;
        }

        /// <summary>
        /// Copies a Stream to an other.
        /// <para>Copie d'un Stream � un autre.</para>
        /// </summary>
        /// <param name="input">The Stream to copy.</param>
        /// <param name="output">the Steam to copy to.</param>
        /// <param name="BufferSize">the buffersize for the copy.</param>
        public static void CopyStream(Stream input, Stream output, int BufferSize, bool flush = true)
        {
            if (BufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(BufferSize), "[HTTPProcessor] - CopyStream() - Buffer size must be greater than zero.");

            bool lockTaken = false;
            int bytesRead = 0;

            try
            {
#if NET5_0_OR_GREATER
                Monitor.TryEnter(_StackLock, ref lockTaken); // Attempt to acquire the lock.

                if (lockTaken) // Lock is free.
                {
                    Span<byte> buffer = stackalloc byte[1024]; // Allocate buffer on the stack (1024 being recommanded value for stackalloc).
                    buffer.Clear();
                    while ((bytesRead = input.Read(buffer)) > 0)
                    {
                        output.Write(buffer[..bytesRead]);
                    }
                    if (flush)
                        output.Flush();
                }
                else
                {
                    Span<byte> buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                    while ((bytesRead = input.Read(buffer)) > 0)
                    {
                        output.Write(buffer[..bytesRead]);
                    }
                    if (flush)
                        output.Flush();
                }
#else
                byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }
                if (flush)
                    output.Flush();
#endif
            }
            finally
            {
                // Release the lock if it was acquired
                if (lockTaken)
                    Monitor.Exit(_StackLock);
            }
        }
#if NET7_0_OR_GREATER
        [GeneratedRegex("^(.*?http://.*?http://)([^/]+)(.*)$")]
        private static partial Regex DirtyProxyRegex();
#endif
    }
}
