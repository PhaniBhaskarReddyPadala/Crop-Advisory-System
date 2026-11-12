// Crop Advisory & Farmer's Companion JS

// Voice Assistant helper using Web Speech API
window.speakText = function(text, lang) {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel(); // Stop any active speech
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = lang || 'en-IN';
        utterance.rate = 0.9;
        window.speechSynthesis.speak(utterance);
    } else {
        console.warn('Speech synthesis not supported in this browser.');
    }
};

window.stopSpeaking = function() {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel();
    }
};
