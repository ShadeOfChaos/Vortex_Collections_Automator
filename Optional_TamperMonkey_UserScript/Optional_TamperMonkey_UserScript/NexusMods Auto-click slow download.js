// ==UserScript==
// @name         NexusMods Auto-click slow download
// @namespace    http://tampermonkey.net/
// @version      2025-02-05
// @description  Automatically clicks the "Slow download"-button on NexusMods
// @author       ShadeOfChaos
// @match        *://www.nexusmods.com/*
// @match        *://nexusmods.com/*
// @icon         data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==
// @grant        none
// ==/UserScript==

(function() {
    'use strict';

    let downloadButton = document.getElementById('slowDownloadButton');
    if(downloadButton != null) {
        console.info('TamperMonkey Script being executed: "NexusMods Auto-click slow download"');
        downloadButton.click();
    }
})();