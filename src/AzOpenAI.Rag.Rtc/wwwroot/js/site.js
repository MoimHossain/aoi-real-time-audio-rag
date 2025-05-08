
function showHtmlElement(id) {
    const element = document.getElementById(id);
    if (element) {
        element.style.display = "block";
    }
}

function hideHtmlElement(id) {
    const element = document.getElementById(id);
    if (element) {
        element.style.display = "none";
    }
}

function logMessage(message) {
    console.log(message);
}

function stopSession() {
    if (window.dataChannel) {
        window.dataChannel.close();
        window.dataChannel = null;
    }
    if (window.peerConnection) {
        window.peerConnection.close();
        window.peerConnection = null;
    }
    if (window.audioElement) {
        window.audioElement.remove();
        window.audioElement = null;
    }

    hideHtmlElement("btnStopSession");
    showHtmlElement("btnBeginSession");
    logMessage("Session closed.");
}

async function createRTCPeerConnection() {
    if (window.peerConnection) {
        logMessage("Session already exists. Closing the existing session.");
        stopSession();
    }
    window.peerConnection = new RTCPeerConnection();
    // Set up to play remote audio from the model response.
    window.audioElement = document.createElement('audio');
    window.audioElement.autoplay = true;
    document.body.appendChild(window.audioElement);
    window.peerConnection.ontrack = (event) => { window.audioElement.srcObject = event.streams[0]; };
    // Set up data channel for sending and receiving events
    const clientMedia = await navigator.mediaDevices.getUserMedia({ audio: true });
    const audioTrack = clientMedia.getAudioTracks()[0];
    window.peerConnection.addTrack(audioTrack);
}

const OAI_RTC_EVENT_HANDLERS = {
    "session.created": async function (realtimeEvent) {
        const event = {
            type: "session.update",
            session: {
                instructions: "You are a helpful AI assistant for Enterprise, English is your default language. You must search the data from Enterprise using 'search' tool and response based on search result. If search result empty, politely decline. But never answer that is not based on search result.",
                tools: [{
                    type: "function",
                    name: "search",
                    description: "Search enterprise information",
                    parameters: {
                        type: "object",
                        properties: {
                            searchKey: {
                                type: "string",
                                description: "Search Key or phrase"
                            }
                        },
                        required: ["searchKey"]
                    }
                }],
                tool_choice: "auto"
            }
        };
        window.dataChannel.send(JSON.stringify(event));
    },
    "session.update": async function (realtimeEvent) {
        if (realtimeEvent.session && realtimeEvent.session.instructions) {
            logMessage("Instructions: " + realtimeEvent.session.instructions);
        }
    },
    "session.error": async function (realtimeEvent) {
        logMessage("Error: " + realtimeEvent.error.message);
    },
    "session.end": async function (realtimeEvent) {
        logMessage("Session ended.");
    },
    "response.done": async function (realtimeEvent) {
        logMessage(realtimeEvent);
    },
    "response.output_item.done": async function (realtimeEvent) {
        if (realtimeEvent.item && realtimeEvent.item.type === "function_call") {
            const item = realtimeEvent.item;
            if (item.name === "search") {                
                logMessage("Searching information...", realtimeEvent);              

                const response = await fetch("/api/search", {
                    method: "POST",
                    body: item.arguments,
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
                if (response.ok) {                    
                    const searchResult = await response.json();
                    logMessage(searchResult);
                    
                    const chatResponse = {
                        type: "conversation.item.create",
                        item: {
                            type: "function_call_output",
                            call_id: item.call_id,
                            output: JSON.stringify(searchResult)
                        }
                    };                
                    window.dataChannel.send(JSON.stringify(chatResponse));

                    setTimeout(() => {
                        window.dataChannel.send(JSON.stringify({
                            type: "response.create"
                        }));
                    }, 100);    
                }
            }
        }
    }
};

async function createDataChannel() {
    window.dataChannel = window.peerConnection.createDataChannel('realtime-channel');

    window.dataChannel.addEventListener('open', () => { logMessage('Data channel is open'); });
    window.dataChannel.addEventListener('close', () => { logMessage('Data channel is closed'); });

    window.dataChannel.addEventListener('message', (event) => {
        const realtimeEvent = JSON.parse(event.data);        
        // find the handler for the event type
        const handler = OAI_RTC_EVENT_HANDLERS[realtimeEvent.type];
        if (handler) { handler(realtimeEvent); }
    });
}

async function createRTCSessionAsync(sessionConfig) {
    await createRTCPeerConnection();
    await createDataChannel();
    // Start the session using the Session Description Protocol (SDP)
    const offer = await window.peerConnection.createOffer();
    await window.peerConnection.setLocalDescription(offer);

    const sdpResponse = await fetch(`${sessionConfig.webRTCUri}?model=${sessionConfig.modelDeploymentName}`, {
        method: "POST",
        body: offer.sdp,
        headers: {
            Authorization: `Bearer ${sessionConfig.session.client_secret.value}`,
            "Content-Type": "application/sdp",
        },
    });
    const answer = { type: "answer", sdp: await sdpResponse.text() };
    await window.peerConnection.setRemoteDescription(answer);
}

async function beginSessionAsync() {
    document.getElementById("errorMessage").innerText = "";
    hideHtmlElement("errorMessageContainer");
    hideHtmlElement("btnBeginSession");
    const selectedVoice = document.getElementById("voiceSelector").value;

    const response = await fetch("/api/session/begin", {
        method: "POST",
        body: JSON.stringify({ "voice": selectedVoice }),
        headers: {
            "Content-Type": "application/json"
        }
    });
    if (response.ok) {
        showHtmlElement("btnStopSession");
        const sessionConfig = await response.json();
        console.log("Session started:", sessionConfig);
        await createRTCSessionAsync(sessionConfig);
    } else {
        document.getElementById("errorMessage").innerText = "Failed to begin session.";
        console.error("Error starting session:", response.status, response.statusText);

        showHtmlElement("errorMessageContainer");
        showHtmlElement("btnBeginSession");
        hideHtmlElement("btnStopSession");
    }
}



/* Examples
       window.dataChannel.send(JSON.stringify({
           type: "conversation.item.create",
           item: {
               type: "message",
               role: 'assistant',
               content: [{
                   type: 'input_text',
                   text: 'Ask the user to wait while we are searching for information'
               }]
           }
       }));
       window.dataChannel.send(JSON.stringify({
           type: "response.create",
           response: {
               instructions: "Wait for search to complete."
           }
       }));
       
       
*/
/*if (realtimeEvent.response
    && realtimeEvent.response.output
    && realtimeEvent.response.output.length > 0) {
    const output = realtimeEvent.response.output[0];
    if (output.type === "function_call") {
        const response = {
            type: "response.create"
        };
        setTimeout(() => {
            window.dataChannel.send(JSON.stringify(response));
        }, 100);
    }
}*/