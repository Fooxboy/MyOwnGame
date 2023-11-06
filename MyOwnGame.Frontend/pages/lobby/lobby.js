//var window.music;
var isAdmin;
var canChooseAnswer;

/*
======================
    Initialization
======================
*/

document.querySelector("#pack-name").textContent = '"' + session.gameInfo.packageName + '"';

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
	document.querySelector("#session-buttons").innerHTML += `
		<div class="button" onclick="connection.invoke('AcceptAnswer')">Верно</div>
		<div class="button" onclick="connection.invoke('RejectAnswer')">Не верно</div>
		<div class="button secondary" onclick="connection.invoke('SkipQuestion')">Скип</div>
	`;
}

document.querySelector("#session-buttons").style.display = isAdmin ? null : "none";
document.querySelector("#answer-button").style.display = isAdmin ? "none" : null;

document.querySelector("#invite-button").addEventListener("click", () => {
	navigator.clipboard.writeText(`${window.location.href}${window.location.href.endsWith("?") ? "" : "?"}invite=${session.id}`);
	alert("Ссылка скопирована");
});


/*
======================
	  From API
======================
*/

function addPlayer(player){
	const imageUrl = `${address}/avatars/${player.avatarImage}`;

	if(player.isAdmin){
		isAdmin = player.id == userId;
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
		profilePane.querySelector(".player-name").innerHTML = player.name;;
		const scoreElement = profilePane.querySelector(".player-score")
		scoreElement.innerHTML = player.score;
		scoreElement.addEventListener("click", () => editPlayerScore(session.players.find(a => a.id == player.id)));
		
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
			const button = document.createElement("div");
			button.id = `price-${r}`;
			button.classList.add("price");
			button.classList.toggle("answered", price.isAnswered);
			button.addEventListener("click", () => {
				if(roundInfo.isFinal)
					requestRemoveTheme(i);
				else if(!price.isAnswered)
					requestPrice(i, r)
			});
			button.textContent = roundInfo.isFinal ? "удалить" : price.price;
			themePanel.appendChild(button);
		});
	});
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
	const answerButton = document.querySelector("#answer-button");
	const priceElement = document.querySelector(`#theme-${position.themeNumber} #price-${position.questionNumber}`);
	priceElement.classList.add("selected");

	setTimeout(() => {
		priceElement.classList.remove("selected");
		priceElement.classList.add("answered");
		processQuestionPart(question, 0);
	}, 1000);

	for(let i = time; i >= 1; i--)
		setTimeout(() => answerButton.innerHTML = i, 1000 * (time - i));
	updatePlayers();
}

function canAnswer(can){
	document.body.classList.toggle("can-answer", can);
	document.querySelector("#answer-button").innerHTML = "Ответить";

	const player = getCachedPlayerById(userId);

	const answerText = document.querySelector("#answer-textfield");
	const answerPricePanel = document.querySelector("#answer-price-panel");
	const isFinalAndCanAnswer = !isAdmin && can && session.currentRound.isFinal;

	answerText.style.display = isFinalAndCanAnswer ? "block" : "none";
	answerPricePanel.style.display = isFinalAndCanAnswer ? "block" : "none";
	
	if(isFinalAndCanAnswer){
		answerText.innerHTML = "";
		setPrice(0, 0, player.score, 1);
	}
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

function updateScore(player, score){
	updatePlayers();
}

function finalThemeRemoved(themes){
	setRound({
		isFinal: true,
		themes: themes
	});
	updatePlayers();
}

function triedToAnswer(player){
	const playerElement = document.querySelector(`#player-${player.id}`);
	playerElement.classList.add("tried-answer");
	setTimeout(() => {
		playerElement.classList.remove("tried-answer");
	}, 300);
}

function finalQuestionResponsed(player){
	updatePlayers();
}

function showUserAnswer(player, answer){
	showContent({
		type: "answer",
		text: answer.answer,
		playerName: player.name
	});
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

function getCachedPlayerById(id){
	return session.players.filter(p2 => p2.id == id)[0];
}

function updatePlayers() {
	requestUpdateSession().then(() => {
		session.players.forEach(player => {
			const imageUrl = `${address}/avatars/${player.avatarImage}`;

			if(player.isAdmin){
				const adminPanel = document.querySelector("#admin-panel");
				adminPanel.querySelector("#admin-image").style.backgroundImage = `url('${imageUrl}')`;
				adminPanel.querySelector("#admin-name").textContent = player.name;
			}else {
				let profilePane = document.querySelector(`#player-${player.id}`);
				
				if(profilePane){
					profilePane.querySelector(".player-image").style.backgroundImage = `url('${imageUrl}')`;
					profilePane.querySelector(".player-name").innerHTML = player.name;
					const scoreElement = profilePane.querySelector(".player-score");
					if(scoreElement.textContent != player.score){
						scoreElement.classList.add("changed");
						setTimeout(() => scoreElement.classList.remove("changed"), 1000);
					}
					scoreElement.innerHTML = player.score;
				}

				if(session.respondingPlayer && 
					(session.currentRound && !session.currentRound.isFinal) &&
					player.id == session.respondingPlayer.id)
					setPlayerStatus(player.id, "отвечает", "rgba(240, 240, 100, 1)", "rgba(120, 120, 0, 1)");
				
				else if(player.isDisconnected)
					setPlayerStatus(player.id, "отключен", "rgba(100, 100, 100, 1)", "rgba(200, 200, 200, 1)");

				else if(
					(session.currentRound && session.currentRound.isFinal) &&
					session.finalAnswers.filter(p => p.id == player.id).length > 0
				){
					setPlayerStatus(player.id, "ответил", "rgba(100, 200, 100, 1)", "rgba(0, 90, 0, 1)");

				}else if(
					(session.currentRound && !session.currentRound.isFinal) &&
					session.state == 4 &&
					(session.selectQuestionPlayer && session.selectQuestionPlayer.id == player.id) 
				) {
					setPlayerStatus(player.id, "выбирает", "rgba(100, 200, 100, 1)", "rgba(0, 90, 0, 1)");
					canChooseAnswer = player.id == userId;
					document.body.classList.toggle("can-choose-price", canChooseAnswer);
				} else setPlayerStatus(player.id, null, null, null);
			}
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
	if(session.currentRound.isFinal){
		connection.invoke("SendFinalAnswer", 
			document.querySelector("#answer-textfield").value,
			parseInt(document.querySelector("#answer-price").value)
		);
		canAnswer(false);
	}else
		connection.invoke("ReadyToAnswer", new Date().toISOString());
}

function requestRemoveTheme(themeId){
	connection.invoke("RemoveFinalTheme", parseInt(themeId));
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
	textQuestion.style.display = (content.type == 1 || content.type == "answer") ? "flex" : "none";
	musicQuestion.style.display = content.type == 2 ? "flex" : "none";
	imageQuestion.style.display = content.type == 3 ? "flex" : "none";

	if(content.type == 1){
		textQuestion.querySelector(`#text-content`).textContent = content.text;
		setTimeout(() => callback(), content.text.length * 200);
	}
	if(content.type == 2){
		window.music = new Audio(`${address}/content/${session.id}/${content.url}`);
		window.music.addEventListener("loadedmetadata", () => {
			setTimeout(() => callback(), window.music.duration * 1000);
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
	if(content.type == "answer"){
		textQuestion.querySelector(`#text-content`).textContent = content.text;
		setAnswerText(content.playerName);
	}
}

function showAnswer(answer) {
	setAnswerText(answer.text);
	showContent(answer, () => {
		showContent(null, null);
	});
	updatePlayers();
}

function editPlayerScore(player){
	if(session.players.find(a => a.id == userId).isAdmin){
		const newScore = prompt("Введите новый счет", player.score);
		if(newScore)
			connection.invoke("SetScore", player.id, parseInt(newScore));
	}
}

function setPrice(price, min = null, max = null, step = 1){
	document.querySelector("#answer-price-value").textContent = price;

	const range = document.querySelector("#answer-price");
	range.value = price;
	if(min) range.min = min;
	if(max) range.max = max;
	range.step = step;
}