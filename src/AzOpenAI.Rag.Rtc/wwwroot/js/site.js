
function getWeather() {
    return {
        temperature: "16",
        unit: "Celsius"
    };
}

function logMessage(message) {
    //const logContainer = document.getElementById("logContainer");
    //const p = document.createElement("p");
    //p.textContent = message;
    //logContainer.appendChild(p);
    console.log(message);
}

function stopSession() {
    if (dataChannel) dataChannel.close();
    if (peerConnection) peerConnection.close();
    peerConnection = null;
    logMessage("Session closed.");
}

function updateSession(dataChannel) {
    const event = {
        type: "session.update",
        session: {
            instructions: "You are a helpful AI assistant responding in natural, English language. When asked about the weather, you must use the get_weather tool to provide the current weather information. Always use the tool, do not make up weather information.",
            tools: [{
                type: "function",
                name: "get_weather",
                description: "Get the current weather information",
                parameters: {
                    type: "object",
                    properties: {
                        query: {
                            type: "string",
                            description: "Weather query"
                        }
                    },
                    required: ["query"]
                }
            }],
            tool_choice: "auto"
        }
    };
    dataChannel.send(JSON.stringify(event));
    logMessage("Sent client event: " + JSON.stringify(event, null, 2));
}

async function createRTCSessionAsync(sessionConfig) {
    let peerConnection = new RTCPeerConnection();
    // Set up to play remote audio from the model.
    const audioElement = document.createElement('audio');
    audioElement.autoplay = true;
    document.body.appendChild(audioElement);

    peerConnection.ontrack = (event) => { audioElement.srcObject = event.streams[0]; };
    // Set up data channel for sending and receiving events
    const clientMedia = await navigator.mediaDevices.getUserMedia({ audio: true });
    const audioTrack = clientMedia.getAudioTracks()[0];
    peerConnection.addTrack(audioTrack);
    const dataChannel = peerConnection.createDataChannel('realtime-channel');

    dataChannel.addEventListener('open', () => {
        logMessage('Data channel is open');
        updateSession(dataChannel);
    });

    dataChannel.addEventListener('message', (event) => {
        const realtimeEvent = JSON.parse(event.data);
        console.log(realtimeEvent);

        if (realtimeEvent.type === "session.update") {
            const instructions = realtimeEvent.session.instructions;
            logMessage("Instructions: " + instructions);
        } else if (realtimeEvent.type === "response.output_item.done" &&
            realtimeEvent.item &&
            realtimeEvent.item.type === "function_call") {
            // Handle tool/function calls
            const item = realtimeEvent.item;
            logMessage("Function call received: " + item.name);

            if (item.name === "get_weather") {
                logMessage("Getting weather information...");
                const weatherResponse = getWeather();
                const response = {
                    type: "conversation.item.create",
                    item: {
                        type: "function_call_output",
                        call_id: item.call_id,
                        output: JSON.stringify(weatherResponse)
                    }
                };
                console.log("Sending weather response:", response);
                dataChannel.send(JSON.stringify(response));
            }
        } else if (realtimeEvent.type === "session.error") {
            logMessage("Error: " + realtimeEvent.error.message);
        } else if (realtimeEvent.type === "session.end") {
            logMessage("Session ended.");
        } else if (realtimeEvent.type === "response.done") {
            if (realtimeEvent.response
                && realtimeEvent.response.output
                && realtimeEvent.response.output.length > 0) {
                const output = realtimeEvent.response.output[0];
                if (output.type === "function_call") {
                    const response = {
                        type: "response.create"
                    };
                    setTimeout(() => {
                        dataChannel.send(JSON.stringify(response));
                    }, 100);
                }
            }
        }
    });

    dataChannel.addEventListener('close', () => {
        logMessage('Data channel is closed');
    });
    // Start the session using the Session Description Protocol (SDP)
    const offer = await peerConnection.createOffer();
    await peerConnection.setLocalDescription(offer);

    const sdpResponse = await fetch(`${sessionConfig.webRTCUri}?model=${sessionConfig.modelDeploymentName}`, {
        method: "POST",
        body: offer.sdp,
        headers: {
            Authorization: `Bearer ${sessionConfig.session.client_secret.value}`,
            "Content-Type": "application/sdp",
        },
    });

    const answer = { type: "answer", sdp: await sdpResponse.text() };
    await peerConnection.setRemoteDescription(answer);
}

async function beginSessionAsync() {
    document.getElementById("errorMessage").innerText = "";
    document.getElementById("errorMessageContainer").style.display = "none";    
    document.getElementById("btnBeginSession").disabled = true;    
    const selectedVoice = document.getElementById("voiceSelector").value;

    const response = await fetch("/api/session/begin", {
        method: "POST",
        body: JSON.stringify({ "voice": selectedVoice }),
        headers: {
            "Content-Type": "application/json"
        }
    });
    if (response.ok) {
        const sessionConfig = await response.json();
        console.log("Session started:", sessionConfig);
        await createRTCSessionAsync(sessionConfig);
    } else {
        document.getElementById("errorMessage").innerText = "Failed to begin session.";        
        document.getElementById("errorMessageContainer").style.display = "block";
        console.error("Error starting session:", response.status, response.statusText);
        document.getElementById("btnBeginSession").disabled = false;
    }
}