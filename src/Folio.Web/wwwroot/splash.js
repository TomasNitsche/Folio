// Intro splash: a spinning 3D cube with the app name. It stays until Blazor has started and rendered the page and
// the cube has finished at least one full turn, is then held for HOLD ms so the name can be read, and finally fades out.
// Timing is clock based (not animation events), so it also works in throttled/background tabs.
(function () {
    var splash = document.getElementById('folio-splash');
    if (!splash) return;

    var DELAY = 200;    // ms, must match the animation delay of .folio-splash-spin in folio.css
    var TURN = 1800;    // ms, must match the animation duration of .folio-splash-spin
    var HOLD = 400;     // ms, extra time the finished splash stays visible before fading out
    var MAX = 20000;    // safety net: never block the app longer than this
    var reduced = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var start = performance.now();
    var done = false;
    var ending = false;

    function appReady() {
        return !!window.Blazor && !!document.querySelector('.app-layout');
    }

    function dismiss() {
        if (done) return;
        done = true;
        splash.classList.add('folio-splash-hide');
        setTimeout(function () { splash.remove(); }, 700);
    }

    function finishSoon() {
        if (ending) return;
        ending = true;
        setTimeout(dismiss, HOLD);
    }

    function tick() {
        if (done || ending) return;
        var elapsed = performance.now() - start;

        if (reduced) {
            if (appReady() && elapsed > 600) return finishSoon();
        } else if (appReady() && elapsed >= DELAY + TURN) {
            // wait for the end of the current turn so the cube never stops mid-rotation
            var intoTurn = (elapsed - DELAY) % TURN;
            if (intoTurn < 120 || elapsed > DELAY + TURN * 3) return finishSoon();
        }

        if (elapsed > MAX) return dismiss();
        setTimeout(tick, 40);
    }

    tick();
})();
