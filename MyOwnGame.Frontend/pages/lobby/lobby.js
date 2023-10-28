var audio;

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


// DEBUG
session.gameInfo.rounds.forEach((round, i) => {
	document.querySelector("#tools").innerHTML += `
		<div class="button" onclick="requestRound(${i})">${round.name}</div>
	`;
});

document.querySelector("#invite-field").value = `${window.location.href}?invite=${session.id}`;


/*
======================
	  From API
======================
*/

function addPlayer(player){
	const imageUrl = `${address}/avatars/${player.avatarImage}`;

	if(player.isAdmin){
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
		else if(player.id == session.selectQuestionPlayer.id)
			setPlayerSelecting(player);
		else
			setPlayerStatus(player.id, null);
	}
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
	setPlayerStatus(player.id, "выбирает", "rgba(100, 200, 100, 1)", "rgba(0, 90, 0, 1)");
}

function setPlayerOffline(player){
	setPlayerStatus(player.id, "отключен", "rgba(100, 100, 100, 1)", "rgba(200, 200, 200, 1)");
}

function showQuestion(question, type, position){
	console.log("Showing question:");
	console.log(question);
	console.log("With type:");
	console.log(type);
	console.log("At position:");
	console.log(position);

	const priceElement = document.querySelector(`#theme-${position.themeNumber} #price-${position.questionNumber}`);
	priceElement.classList.add("selected");

	setTimeout(() => {
		priceElement.classList.remove("selected");
		processQuestionPart(question, 0);
	}, 1000);
}

/*
======================
	  Functions
======================
*/

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

function setVolume(volume){
	audio.volume = volume / 100;
}

function processQuestionPart(parts, i){
	if(i == parts.length){
		return;
	}
	const part = parts[i];

	const questions = document.querySelector(`#questions`);
	const imageQuestion = document.querySelector(`#image-question`);
	const textQuestion = document.querySelector(`#text-question`);
	const musicQuestion = document.querySelector(`#music-question`);

	questions.style.display = "none";
	textQuestion.style.display = part.type == 1 ? "flex" : "none";
	musicQuestion.style.display = part.type == 2 ? "flex" : "none";
	imageQuestion.style.display = part.type == 3 ? "flex" : "none";

	if(part.type == 1){
		textQuestion.querySelector(`#text-content`).textContent = part.text;
		setTimeout(() => processQuestionPart(parts, i+1), part.text.length * 200);
	}
	if(part.type == 2){
		audio = new Audio(`${address}/content/${session.id}/${part.url}`);
		audio.addEventListener("loadedmetadata", () => {
			setTimeout(() => processQuestionPart(parts, i+1), audio.duration * 1000);
		});
		audio.play();
	}
	if(part.type == 3){
		imageQuestion.querySelector(`#image-content`).style.backgroundImage 
			= `url("${address}/content/${session.id}/${part.url}")`;
		setTimeout(() => processQuestionPart(parts, i+1), 3000);
	}
}