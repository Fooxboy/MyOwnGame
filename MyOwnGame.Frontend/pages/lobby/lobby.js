//var window.music;
var isAdmin;
var canChooseAnswer;

/*
======================
    Initialization
======================
*/

session.players.forEach(player => {
	addPlayer(player);
});
if(session.currentRound)
	setRound(session.currentRound);
if(session.selectQuestionPlayer)
	setPlayerSelecting(session.selectQuestionPlayer);
if(session.respondingPlayer)
	setPlayerAnswer(session.respondingPlayer);

// DEBUG
if(isAdmin){
	session.gameInfo.rounds.forEach((round, i) => {
		document.querySelector("#tools").innerHTML += `
			<div class="button" onclick="requestRound(${i})">${round.name}</div>
		`;
	});
	document.querySelector("#tools").innerHTML += `
		<div class="button" onclick="connection.invoke('AcceptAnswer')">Верно</div>
		<div class="button" onclick="connection.invoke('RejectAnswer')">Не верно</div>
		<div class="button" onclick="connection.invoke('SkipQuestion')">Скип</div>
	`;
}

document.querySelector("#invite-field").value = `${window.location.href}?invite=${session.id}`;


/*
======================
	  From API
======================
*/

function addPlayer(player){
	const imageUrl = `${address}/avatars/${player.avatarImage}`;

	if(player.isAdmin){
		isAdmin = player.id == userId;
		const adminPanel = document.querySelector("#admin-panel");
		if(!adminPanel) {
			adminPanel = document.createElement("div");
			adminPanel.classList = "admin-panel";
			document.querySelector("#left-panel").appendChild(adminPanel);
		}

		adminPanel.innerHTML = `
			<div class="admin-label">Ведущий</div>
			<div class="admin-image" style="background-image: url('${imageUrl}')"></div>
			<div class="admin-name">${player.name}</div>
		`;
	} else {
		let profilePane = document.querySelector(`#player-${player.id}`);
		if(!profilePane) {
			profilePane = document.createElement("div");
			profilePane.classList = "player";
			profilePane.id = `player-${player.id}`;
			document.querySelector("#players").appendChild(profilePane);

			profilePane.innerHTML = `
				<div class="player-status"></div>
				<div class="player-info">
					<div class="player-image"></div>
					<div class="player-name"></div>
				</div>
				<div class="player-score"></div>
			`;
		}
		profilePane.querySelector(".player-image").style.backgroundImage = `url('${imageUrl}')`;
		profilePane.querySelector(".player-name").innerHTML = player.name;
		profilePane.querySelector(".player-score").innerHTML = player.score;
		
		if(player.isDisconnected)
			setPlayerOffline(player);
		else if(session.selectQuestionPlayer && player.id == session.selectQuestionPlayer.id)
			setPlayerSelecting(player);
		else
			setPlayerStatus(player.id, null);
	}

	updatePlayers();
}

function removePlayer(player){
	if(player.isAdmin)
		document.querySelector("#admin-panel")?.remove();
	else
		document.querySelector(`#player-${player.id}`)?.remove();
}

function setRound(roundInfo){
	const questions = document.querySelector("#questions");
	questions.innerHTML = "";

	roundInfo.themes.forEach((theme, i) => {
		let themePanel = document.createElement("div");
		themePanel.classList = "theme-line";
		themePanel.id = `theme-${i}`;
		questions.appendChild(themePanel);

		themePanel.innerHTML += `
			<div class="theme-name">${theme.name}</div>
		`;

		theme.prices.forEach((price, r) => {
			themePanel.innerHTML += `
				<div class="price" id="price-${r}" onclick="requestPrice(${i}, ${r})">${roundInfo.isFinal ? "удалить" : price.price}</div>
			`;
		});
	});

	console.log(roundInfo);
}

function setPlayerSelecting(player){
	updatePlayers();
}

function setPlayerOffline(player){
	updatePlayers();
}

function setPlayerAnswer(player){
	updatePlayers();
}

function showQuestion(question, type, time, position){
	console.log("Showing question:");
	console.log(question);
	console.log("With type:");
	console.log(type);
	console.log("At position:");
	console.log(position);

	const answerButton = document.querySelector(".answer-button");
	const priceElement = document.querySelector(`#theme-${position.themeNumber} #price-${position.questionNumber}`);
	priceElement.classList.add("selected");

	setTimeout(() => {
		priceElement.classList.remove("selected");
		processQuestionPart(question, 0);
	}, 1000);

	const seconds = 5;
	for(let i = seconds; i >= 1; i--)
		setTimeout(() => answerButton.innerHTML = i, 1000 * (seconds - i));
	updatePlayers();
}

function canAnswer(can){
	document.body.classList.toggle("can-answer", can);
	document.querySelector(".answer-button").innerHTML = "Ответить";
}

function showAdminAnswer(answer){
	setAnswerText(answer.text);
}

function acceptAnswer(player, score, answer) {
	showAnswer(answer);
}

function rejectAnswer(player, score, answer) {
	updatePlayers(); // TODO: Animation
}

function skipQuestion(answer) {
	showAnswer(answer);
}

/*
======================
	  Functions
======================
*/

function requestUpdateSession(){
	const id = parseInt(session.id);
	return connection.invoke("GetSession", id)
	.then(result => {
		session = result;
		session.id = id;
		return this;
	});
}

function getCachedPlayer(player){
	return session.players.filter(p2 => p2.id == player.id);
}

function updatePlayers() {
	requestUpdateSession().then(() => {
		session.players.forEach(player => {
			let profilePane = document.querySelector(`#player-${player.id}`);
			const imageUrl = `${address}/avatars/${player.avatarImage}`;
			if(profilePane){
				profilePane.querySelector(".player-image").style.backgroundImage = `url('${imageUrl}')`;
				profilePane.querySelector(".player-name").innerHTML = player.name;
				profilePane.querySelector(".player-score").innerHTML = player.score;
			}

			if(session.respondingPlayer && 
				player.id == session.respondingPlayer.id)
				setPlayerStatus(player.id, "отвечает", "rgba(240, 240, 100, 1)", "rgba(120, 120, 0, 1)");
			
			else if(player.isDisconnected)
				setPlayerStatus(player.id, "отключен", "rgba(100, 100, 100, 1)", "rgba(200, 200, 200, 1)");
			
			else if(session.selectQuestionPlayer && 
				session.state == 4 &&
				player.id == session.selectQuestionPlayer.id) {
				setPlayerStatus(player.id, "выбирает", "rgba(100, 200, 100, 1)", "rgba(0, 90, 0, 1)");
				canChooseAnswer = player.id == userId;
				document.body.classList.toggle("can-choose-price", canChooseAnswer);
			} else setPlayerStatus(player.id, null, null, null);
		});
	});
}

function getPlayerStatus(id){
	return document.querySelector(`#player-${id} .player-status`)?.innerHTML;
}

function setPlayerStatus(id, status, bg, color){
	const profilePane = document.querySelector(`#player-${id}`);
	if(profilePane){
		profilePane.classList.toggle("has-status", status != null);

		const statusPanel = profilePane.querySelector(".player-status");
		statusPanel.innerHTML = status;
		statusPanel.style.backgroundColor = bg;
		statusPanel.style.color = color;
	}
}

function requestRound(roundIndex){
	connection.invoke("ChangeRound", roundIndex);
}

function requestPrice(theme, price){
	connection.invoke("SelectQuestion", theme, price);
}

function requestAnswer(){
	connection.invoke("ReadyToAnswer", new Date().toISOString());
}

function setVolume(volume){
	window.music.volume = volume / 100;
}

function setAnswerText(text){
	const textElement = document.querySelector("#answer-text");
	textElement.style.display = text ? "block" : "none";
	textElement.innerHTML = text;
}

function processQuestionPart(parts, i){
	if(i == parts.length)
		return;
	showContent(parts[i], () => processQuestionPart(parts, i+1));
}

function showContent(content, callback) {
	const questions = document.querySelector(`#questions`);
	const imageQuestion = document.querySelector(`#image-question`);
	const textQuestion = document.querySelector(`#text-question`);
	const musicQuestion = document.querySelector(`#music-question`);

	if(content == null){
		questions.style.display = "flex";
		textQuestion.style.display = "none";
		musicQuestion.style.display = "none";
		imageQuestion.style.display = "none";
		setAnswerText(null);
		if(window.music)
			window.music.pause();
		return;
	}

	questions.style.display = "none";
	textQuestion.style.display = content.type == 1 ? "flex" : "none";
	musicQuestion.style.display = content.type == 2 ? "flex" : "none";
	imageQuestion.style.display = content.type == 3 ? "flex" : "none";

	if(content.type == 1){
		textQuestion.querySelector(`#text-content`).textContent = content.text;
		setTimeout(() => callback(), content.text.length * 200);
		
	}
	if(content.type == 2){
		window.music = new Audio(`${address}/content/${session.id}/${content.url}`);
		window.music.addEventListener("loadedmetadata", () => {
			setTimeout(() => callback(), audio.duration * 1000);
		});
		window.music.play();
		if(content.text != null)
			setAnswerText(content.text);
	}
	if(content.type == 3){
		imageQuestion.querySelector(`#image-content`).style.backgroundImage 
			= `url("${address}/content/${session.id}/${content.url}")`;
		setTimeout(() => callback(), 3000);
		if(content.text != null)
			setAnswerText(content.text);
	}
}

function showAnswer(answer) {
	setAnswerText(answer.text);
	showContent(answer, () => {
		showContent(null, null);
	});
	updatePlayers();
}