function bannerInit(elementId,style) {
    window.onload = function () {
        var googlePlayUrl = "https://play.google.com/store/apps/details?id=com.lasergroup.ami";
		var appleStoreUrl = "https://itunes.apple.com/it/app/visitami/id667357331";
		var siteUrl = "http://www.anfiteatromorenicoivrea.it";
		var scriptsPath = "/scripts/";
		var styleImagesPath = "/styles/images/banner/";

		var head = document.getElementsByTagName('head')[0];
		var container = document.getElementById(elementId);
		if (head != null && container != null) {
		    var s = document.createElement('link');
		    s.setAttribute('type', 'text/css');
		    s.setAttribute('rel', 'stylesheet');
		    s.setAttribute('href', 'banner.css');
		    head.appendChild(s);
		    container.setAttribute('class', 'krakebanner ' + style);
		    container.innerHTML = '<div class="krakebanner-container"><div class="krakebanner-background"><div class="krakebanner-logo"><a href="' + siteUrl + '"  target="_blank"><img src="' + siteUrl + styleImagesPath + 'logo.png" /></a></div><div class="krakebanner-stores"><a href="' + googlePlayUrl + '" target="_blank"><img src="' + siteUrl + styleImagesPath + 'android.png" class="krakebanner-googleplay" /></a> <a href="' + appleStoreUrl + '"  target="_blank"><img src="' + siteUrl + styleImagesPath + 'apple.png" class="krakebanner-applestore" /></a></div></div></div>';
		}
	}
}
