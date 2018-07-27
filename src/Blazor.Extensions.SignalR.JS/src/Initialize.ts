import { HubConnectionManager } from './HubConnectionManager';

const blazorExtensions = 'BlazorExtensions';

function initialize() {
  "use strict";

  if (typeof window !== 'undefined' && !window[blazorExtensions]) {
    // When the library is loaded in a browser via a <script> element, make the
    // following APIs available in global scope for invocation from JS
    window[blazorExtensions] = {
      SignalR: new HubConnectionManager()
    };
  } else {
    window[blazorExtensions] = {
      ...window[blazorExtensions],
      SignalR: new HubConnectionManager()
    };
  }
}

initialize();
