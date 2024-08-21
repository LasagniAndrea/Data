// Callback to set the div width to 100% after any resize event
var resetWidth = function(e) {

	var oDiv1 = document.getElementById("pnlTracker1");
	if (null != oDiv1)
        oDiv1.style.width = '100%';
    
    var oDiv2 = document.getElementById("pnlTracker2");
    if (null != oDiv2)
        oDiv2.style.width = '100%';
    
    var oDiv3 = document.getElementById("pnlRequester");
    if (null != oDiv3)
        oDiv3.style.width = '100%';
   
}

// Make the tracker div resizable
function InitResize() {

    var Dom = YAHOO.util.Dom,
        Event = YAHOO.util.Event;
    
    var pnlTracker1 = new YAHOO.util.Resize('pnlTracker1', {
        proxy : true,
        handles: ['b']
    });
    
    var pnlTracker2 = new YAHOO.util.Resize('pnlTracker2', {
        proxy : true,
        handles: ['b']
    });

    var pnlRequester = new YAHOO.util.Resize('pnlRequester', {
        proxy : true,
        handles: ['b']
    });
    
    YAHOO.util.Event.addListener('tblForm', 'resize', resetWidth);
}

