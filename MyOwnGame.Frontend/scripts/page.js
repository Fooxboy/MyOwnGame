const address = "https://fooxboy.ru:3000";
let connection;
let session;

let choosenAvatarFile = 0;
let userId = 0;
let userImage = "";
let userName = "";

function chooseAvatar(){
	var input = document.createElement('input');
	input.type = 'file';
	input.onchange = e => { 
		choosenAvatarFile = e.target.files[0];

		var reader = new FileReader();
		reader.onload = () => {
			document.body.style.setProperty("--user-image", 'url("' + reader.result + '")');
		};
		reader.readAsDataURL(e.target.files[0]);
	}
	input.click();
}

function loadAccountFromCookie(){
	userId = getCookie("user-id");
	userImage = getCookie("user-image");
	userName = getCookie("user-name");
	const lastSession = getCookie("last-session");

	if(userId != null){
		console.log(`Load from cache:`);
		console.log(`\tid: ${userId}`);
		console.log(`\tuserImage: ${userImage}`);
		console.log(`\tuserName: ${userName}`);
		console.log(`\tlastSession: ${lastSession}`);

		document.body.style.setProperty("--user-image", `url("${address}/avatars/${userImage}")`);

		// Get invite parameter if it exist
		const invite = new URLSearchParams(window.location.search).get('invite');

		if(invite){
			removeURLParam("invite");
			connectToSession(invite);
		}
		else if(lastSession)
			connectToSession(lastSession);
		else
			setPage("main");
	}
	else {
		const baseUrl = location.protocol + '//' + location.host + location.pathname;
		document.body.style.setProperty("--user-image", `url("${baseUrl}resources/empty-user-icon.png")`);
		setPage("login");
	}
}

function createAccount(){
	const formData = new FormData();
	formData.append('image', choosenAvatarFile);

	const nameField = document.querySelector("#user-name-field");
  
	fetch(`${address}/users/create?name=${nameField.value}`, {
	  	method: "POST",
	  	body: formData
	}).then(a => a.json())
	.then(result => {
		setCookie("user-id", result.id);
		setCookie("user-image", result.avatarImage);
		setCookie("user-name", result.name);
		loadAccountFromCookie();
	});
}

function createSession(){
	var input = document.createElement('input');
	input.type = 'file';
	input.onchange = e => {
		const formData = new FormData();
		formData.append('package', e.target.files[0]);

		setPage("loading");

		fetch(`${address}/sessions/createSession`, {
		  	method: "POST",
		  	body: formData
		}).then(a => a.json())
		.then(result => {
			console.log(result);
			connectToSession(result.sessionId);
		})
		.catch(a => {
			setPage("main");
		});
	};
	input.click();
}

async function connectToSession(sessionId){
	console.log("Connecting to server...");
	connection = new signalR.HubConnectionBuilder()
		.withUrl(`${address}/hubs/session`, { withCredentials: false })
		.configureLogging(signalR.LogLevel.Information)
		.build();

	connection.onclose(() => disconnect());

    try {
        await connection.start();
        console.log("Connected!");

		session = await connection.invoke("ConnectToSession", parseInt(sessionId), parseInt(userId));
		connection.on("PlayerConnectedToSession", player => addPlayer(player));
		connection.on("PlayerDisconnectedFromSession", player => removePlayer(player));
		connection.on("RoundChanged", roundInfo => setRound(roundInfo));
		connection.on("ChangeSelectQuestionPlayer", player => setPlayerSelecting(player));
		connection.on("PlayerOffline", player => setPlayerOffline(player));
		connection.on("QuestionSelected", (question, type, time, position) => showQuestion(question, type, time, position));
		connection.on("PlayerCanAnswer", () => canAnswer(true));
		connection.on("PlayerAnswer", player => setPlayerAnswer(player));
		connection.on("QuestionSelectedAdmin", answer => showAdminAnswer(answer));
		connection.on("AcceptAnswer", (player, score, answer) => acceptAnswer(player, score, answer));
		connection.on("RejectAnswer", (player, score, answer) => rejectAnswer(player, score, answer));
		connection.on("SkipQuestion", answer => skipQuestion(answer));


		session["id"] = sessionId;
		setCookie("last-session", sessionId);
		setPage("lobby");
        
        console.log(session);
    } catch (err) {
        console.log(err);
        disconnect();
    }
}

async function disconnect(){
	try {
		await connection.invoke("DisconnectFromSession");
	} catch(e){}
	await connection.stop();
	setPage("main");
	setCookie("last-session", null);
}