function removeURLParam(parameter) {
	var url = document.location.href;
	var urlparts = url.split('?');

	if (urlparts.length >= 2) {
		var urlBase = urlparts.shift(); 
		var queryString = urlparts.join("?"); 

		var prefix = encodeURIComponent(parameter) + '=';
		var pars = queryString.split(/[&;]/g);
		for (var i = pars.length; i-->0;)               
			if (pars[i].lastIndexOf(prefix, 0) !== -1)   
				pars.splice(i, 1);
		url = urlBase + '?' + pars.join('&');
		window.history.pushState('', document.title, url);
	}
	return url;
}