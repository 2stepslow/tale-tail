# Tale Tail 프로젝트 지침

## 출력 언어

- 코드 주석, 진행 메시지, 최종 보고는 모두 한국어로 쓴다. 주석은 짧게.
- UI에 표시되는 문구(버튼, 제목, 안내)는 한국어로 쓴다.

## 프로젝트 개요

한 줄 일기의 감정을 파인튜닝한 한국어 감성 모델이 판정하고, 브라우저 속 유니티 WebGL 펫이 반응하는 하이브리드 웹 육성 게임.
구조: 웹(HTML/CSS/JS) + 유니티 WebGL 캔버스(C#) + FastAPI 모델 서버(Python).

## 환경

- macOS (Apple Silicon), Unity 6000.3.20f1, 빌드 대상 Web(WebGL)
- 개발 중 Player Settings의 Compression Format은 Disabled (배포 시 재검토)
- 클로드코드는 TaleTail 폴더에서 실행한다.
- 유니티 6.3 기본 Web 템플릿 원본 위치: /Applications/Unity/Hub/Editor/6000.3.20f1/PlaybackEngines/WebGLSupport/BuildTools/WebGLTemplates/Base/Default (Base 계층 있음)
- 개발 확인 브라우저는 크롬. Build Profiles의 Client Browser Type이 Google Chrome, 경로는 앱 묶음(/Applications/Google Chrome.app)으로 설정되어 있음

## 폴더 구조

- TaleTail/ : 유니티 프로젝트 (2D). Assets/Scripts에 C#, Assets/WebGLTemplates/TaleTail에 웹 코드
- data/ : AI Hub 말뭉치. 절대 읽거나 수정하지 말 것 (재배포 금지 데이터)
- notebooks/ : 실험 노트북
- server/ : FastAPI 서버 (페이즈 3에서 생성 예정)
- Builds/, Library/, Temp/, Logs/ : 산출물·캐시. 읽지도 수정하지도 않는다.

## 유니티 에디터 조작 규칙

- 에디터 조작(설정 변경, 씬 구성, 오브젝트·컴포넌트 조작)은 사용자가 프롬프트에서 위임한 항목만 unity-editor-mcp 도구로 수행한다. 위임 지시가 없는 조작은 하지 말고, 사용자가 직접 할 수 있도록 정확한 메뉴 경로와 순서를 안내한다.
- 에디터 상태, 씬 정보, 콘솔 로그 조회는 파일이나 로그 파일을 뒤지지 말고 MCP 도구로 한다. 조회는 위임 지시 없이도 수행한다.
- eval(unity command eval)은 값을 읽는 조회 용도로만 위임 없이 사용할 수 있다. 에디터나 에셋의 상태를 바꾸는 eval은 프롬프트에 명시적으로 위임된 경우에만 실행하고, 실행 전 어떤 코드를 실행할지 보고한다.
- ProjectSettings/*.asset 과 *.unity 씬 파일을 텍스트로 직접 편집하지 않는다.
- 유니티 에디터가 켜져 있어야 MCP가 동작한다. 연결이 안 되면 파일 편집으로 대체하지 말고 그 사실을 보고한다.
- 코드 변경 후에는 get_console_logs로 컴파일 오류 여부를 확인하고 보고에 포함한다.

## 웹 코드 규칙

- 웹 코드의 정식 위치는 TaleTail/Assets/WebGLTemplates/TaleTail/ 이다. Builds/ 안의 파일은 수정하지 않는다.
- index.html과 app.js의 유니티 템플릿 변수({{{ ... }}}, #if)와 createUnityInstance 로더 코드는 항상 보존한다.
- 구조는 index.html, style.css, app.js로 분리하고 상대 경로로 참조한다.
- 웹에서 유니티로의 전달은 window.unityInstance.SendMessage를 사용한다.
- 유니티 로더(createUnityInstance 호출부)는 index.html이 아니라 app.js에 있다. 로딩 관련 수정은 app.js에서 한다.
- app.js에 남아 있는 유니티 매크로({{{ }}}, #if)는 빌드 시 치환되는 것이므로 VS Code의 JS 오류 표시와 무관하게 그대로 둔다.

## 작업 규칙

- 깃 명령(commit, push, branch, merge)은 실행하지 않는다. 형상관리는 사용자가 GitKraken으로 직접 한다.
- 이해가 필요한 코드에는 설계 이유를 한 줄 덧붙인다.
- 작업 완료 시 다음을 한국어로 보고한다: 생성·수정한 파일 목록, 사용자가 직접 확인해야 할 항목, 콘솔 오류 여부.
- Assets 안의 파일을 삭제하거나 이동할 때는 짝이 되는 .meta 파일도 함께 처리한다. 임시 백업 파일을 Assets 안에 만들지 않는다.

## 아키텍처 원칙

- 유니티는 펫의 표현과 상태(상태 머신)만 담당한다. 입력·회고·서버 통신은 웹이 담당한다.
- 감정 판정은 긍정/부정/중립 3분류. 판정 로직은 교체 가능하게 추상화한다(키워드 → 모델 호출).
- 진단·판정성 표현(우울 위험 감지 등)은 어떤 대사에도 넣지 않는다.
