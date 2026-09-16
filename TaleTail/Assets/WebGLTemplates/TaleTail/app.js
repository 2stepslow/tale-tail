// TaleTail 웹 페이지 로직: 유니티 로더 + 드로어 + 한 줄 일기 폼

var canvas = document.querySelector("#unity-canvas");

// Shows a temporary message banner/ribbon for a few seconds, or
// a permanent error message on top of the canvas if type=='error'.
// If type=='warning', a yellow highlight color is used.
// Modify or remove this function to customize the visually presented
// way that non-critical warnings and error messages are presented to the
// user.
function unityShowBanner(msg, type) {
  var warningBanner = document.querySelector("#unity-warning");
  function updateBannerVisibility() {
    warningBanner.style.display = warningBanner.children.length ? 'block' : 'none';
  }
  var div = document.createElement('div');
  div.innerHTML = msg;
  warningBanner.appendChild(div);
  if (type == 'error') div.style = 'background: red; padding: 10px;';
  else {
    if (type == 'warning') div.style = 'background: yellow; padding: 10px;';
    setTimeout(function() {
      warningBanner.removeChild(div);
      updateBannerVisibility();
    }, 5000);
  }
  updateBannerVisibility();
}

var buildUrl = "Build";
var loaderUrl = buildUrl + "/{{{ LOADER_FILENAME }}}";
var config = {
  arguments: [],
  dataUrl: buildUrl + "/{{{ DATA_FILENAME }}}",
  frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}",
#if USE_THREADS
  workerUrl: buildUrl + "/{{{ WORKER_FILENAME }}}",
#endif
#if USE_WASM
  codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}",
#endif
#if SYMBOLS_FILENAME
  symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}",
#endif
  streamingAssetsUrl: "StreamingAssets",
  companyName: {{{ JSON.stringify(COMPANY_NAME) }}},
  productName: {{{ JSON.stringify(PRODUCT_NAME) }}},
  productVersion: {{{ JSON.stringify(PRODUCT_VERSION) }}},
  showBanner: unityShowBanner,
};

if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
  var meta = document.createElement('meta');
  meta.name = 'viewport';
  meta.content = 'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
  document.getElementsByTagName('head')[0].appendChild(meta);
  document.querySelector("#unity-container").className = "unity-mobile";
  canvas.className = "unity-mobile";

#if SHOW_DIAGNOSTICS
  // position the diagnostics icon in the corner on the canvas
  let diagnostics_icon = document.getElementById("diagnostics-icon");
  diagnostics_icon.style.position = "fixed";
  diagnostics_icon.style.bottom = "10px";
  diagnostics_icon.style.right = "0px";
  canvas.after(diagnostics_icon);
#endif

}
// 캔버스 크기는 style.css가 정한다(폭 100% + 빌드 해상도 비율).
// 원본 템플릿처럼 px를 인라인으로 박으면 드로어가 열릴 때 캔버스가 줄지 않아 가려진다.

#if BACKGROUND_FILENAME
canvas.style.background = "url('" + buildUrl + "/{{{ BACKGROUND_FILENAME.replace(/'/g, '%27') }}}') center / cover";
#endif
document.querySelector("#unity-loading-bar").style.display = "block";

var script = document.createElement("script");
script.src = loaderUrl;
script.onload = () => {
  createUnityInstance(canvas, config, (progress) => {
    document.querySelector("#unity-progress-bar-full").style.width = 100 * progress + "%";
        }).then((unityInstance) => {
          document.querySelector("#unity-loading-bar").style.display = "none";
#if SHOW_DIAGNOSTICS
          document.getElementById("diagnostics-icon").onclick = () => {
            unityDiagnostics.openDiagnosticsDiv(unityInstance.GetMetricsInfo);
          };
#endif
          document.querySelector("#unity-fullscreen-button").onclick = () => {
            unityInstance.SetFullscreen(1);
          };

          // 웹에서 SendMessage로 펫에 신호를 보내야 하므로 전역에 보관한다
          window.unityInstance = unityInstance;

#if DEVELOPMENT_PLAYER
          var profile = unityProfiler.createButton(unityInstance);
          profile.style.marginLeft = '5px';
          document.querySelector("#unity-build-title").appendChild(profile);

          // Unloading web content from DOM so that browser GC can run can be tricky to get right.
          // This code snippet shows how to correctly implement a Unity content Unload mechanism to a web page.

          // Unloading Unity content enables a web page to reclaim the memory used by Unity, e.g. for
          // the purpose of later loading another Unity content instance on the _same_ web page.

          // When using this functionality, take caution to carefully make sure to clear all JavaScript code,
          // DOM element and event handler references to the old content you may have retained, or
          // otherwise the browser's garbage collector will be unable to reclaim the old page.

          // N.b. Unity content does _not_ need to be manually unloaded when the user is navigating away from
          // the current page to another web page. The browser will take care to clear memory of old visited
          // pages automatically. This functionality is only needed if you want to switch between loading
          // multiple Unity builds on a single web page.
          var quit = document.createElement("button");
          quit.style = "margin-left: 5px; background-color: lightgray; border: none; padding: 5px; cursor: pointer";
          quit.innerHTML = "Unload";
          document.querySelector("#unity-build-title").appendChild(quit);
          quit.onclick = () => {
            // Quit Unity application execution
            unityInstance.Quit().then(() => {
              unityProfiler.shutDown();
              // Remove DOM elements from the page so GC can run
              document.querySelector("#unity-container").remove();
              canvas = null;
              // Remover script elements from the page so GC can run
              script.remove();
              script = null;
            });
          };
#endif
        }).catch((message) => {
          alert(message);
        });
      };

document.body.appendChild(script);


// ---- 드로어 ----
// 캔버스를 숨기거나 DOM에서 떼어내지 않고 위에 겹쳐 여닫는다.
// (display:none 이나 언마운트는 WebGL 컨텍스트 렌더링을 멈추게 한다.)
var drawer = document.querySelector("#drawer");
var drawerOpenButton = document.querySelector("#drawer-open");
var drawerCloseButton = document.querySelector("#drawer-close");

function setDrawer(open) {
  drawer.classList.toggle("open", open);
  document.body.classList.toggle("drawer-open", open);
  drawerOpenButton.setAttribute("aria-expanded", String(open));
  // 닫힌 동안에는 탭 이동으로 들어가지 못하게 막는다
  drawer.inert = !open;
}

drawerOpenButton.addEventListener("click", () => setDrawer(true));
drawerCloseButton.addEventListener("click", () => setDrawer(false));
document.addEventListener("keydown", (e) => {
  if (e.key === "Escape" && drawer.classList.contains("open")) setDrawer(false);
});

setDrawer(false);


// ---- 한 줄 일기 폼 ----
// 아직 저장·판정 경로가 없으므로 값만 확인하고 넘어간다.
var diaryForm = document.querySelector("#diary-form");
var diaryInput = document.querySelector("#diary-input");

diaryForm.addEventListener("submit", (e) => {
  e.preventDefault();
  var text = diaryInput.value.trim();
  if (!text) return;
  console.log("[TaleTail] 일기 입력:", text);
});
