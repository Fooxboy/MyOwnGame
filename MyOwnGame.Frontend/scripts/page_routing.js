function setPage(page){
	fetch(`pages/${page}/${page}.html`)
	.then(a => a.text())
	.then(response => {
		// Style
		const link = document.createElement("link");
		link.href = `pages/${page}/${page}.css`;
		link.rel = "stylesheet";
		link.id = "dynamic-page-style";

		const oldLink = document.head.querySelector("link#dynamic-page-style");
		if(oldLink)
			document.head.removeChild(oldLink);
		document.head.appendChild(link);

		// Html
		document.body.innerHTML = response;

		// Script
		fetch(`pages/${page}/${page}.js`)
		.then(a => a.text())
		.then(response => {
			const script = document.createElement("script");
			script.id = "dynamic-page-style";
			script.innerHTML = response;

			const oldScript = document.head.querySelector("script#dynamic-page-style");
			if(oldScript) 
				document.head.removeChild(oldScript);
			document.head.appendChild(script);
		});
	});
}