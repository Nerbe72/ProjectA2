# ProjectA2 — Advanced C# Techniques Inventory

작성일: 2025-09-08 03:00 (KST)
대상: Unity C# 코드베이스 `Assets/Scripts/`
목적: 프로젝트 전역에 흩어진 중·고급 C# 구현 기법을 중복 없이 통합 정리하고, 각 기법이 어떤 시스템/기능을 위해 사용되는지 연결하여 아키텍처 파악과 문서화를 지원한다.

---

## 문서 읽는 법
- 각 섹션은 “기법(패턴/테크닉)” 단위로 구성되었습니다.
- 하위에는 다음 정보를 제공합니다.
  - 요약: 기법의 핵심 의도/효과
  - 주요 참조: 파일/클래스/메서드(대표 위치)
  - 지원 기능: 해당 기법이 실질적으로 기여하는 게임/시스템 기능
  - 설계 관점: OOP/SOLID/패턴 관점의 포인트
  - 개선 제안: 유지보수/성능/확장성 향상을 위한 선택지

코드 표기는 백틱으로 감쌉니다. 예: `Assets/Scripts/System/Manager/TimeController.cs`의 `PollTimeCoroutine()`

---

## 1) 싱글톤 서비스 로케이터 + 수명 관리
- 요약
  - 전역 접근이 필요한 매니저를 단일 인스턴스로 유지하고, 씬 전환 간 생존을 보장하며, 우선순위 기반 해제를 지원한다.
- 주요 참조
  - `Assets/Scripts/System/Manager/Singleton.cs`
  - 적용 예: `TimeController`, `AuthManager`, `PostProcessingManager`, `DialogueManager`, `GameManager`, `ItemFactory`, `EnemyManager`, `CameraManager`, `PhotonManager` 등
- 지원 기능
  - 전역 상태/서비스 제공, 씬 간 지속, 역순 언로드(`UnloadAllSingleton()`)
- 설계 관점
  - Service Locator + Singleton 패턴. DIP 일부 준수(런타임 획득), 전역 상태로 인한 테스트 난이도 상승 가능성은 유의.
- 개선 제안
  - 인터페이스 기반 등록/조회 강화, 에디터/테스트 환경에서 Mock 주입 경로 추가, Lazy 초기화 정책 명시화.

## 2) 코루틴–Task 브리지 비동기 흐름
- 요약
  - `async/await` Task와 Unity 코루틴을 혼합해 프레임 친화적인 비동기 로직을 구성한다.
- 주요 참조
  - `Assets/Scripts/System/Manager/TimeController.cs` → `PollTimeCoroutine()`
  - `Assets/Scripts/System/LoadingScene/SceneLoader.cs` → 로딩 시퀀스 전반
  - `Assets/Scripts/System/Manager/ResourceLoader.cs` → `LoadAsync`
- 지원 기능
  - 서버/IO 대기 중 UI/게임 진행이 끊기지 않도록 자연스러운 프레임 진행
- 설계 관점
  - 비동기 합성(코루틴 <-> Task)으로 응답성 향상. 로직 경계 명확화가 중요.
- 개선 제안
  - 공통 브리지 유틸(예: `AwaitUntilCompleted<T>(Task<T>)`)로 반복 패턴 축소, 타임아웃/취소 토큰 표준화.

## 3) 네트워크 요청 추상화(권한/공개) + 제네릭 직렬화
- 요약
  - 인증 토큰 주입, 공통 에러 처리, 제네릭 JSON 직렬화/역직렬화 추상화.
- 주요 참조
  - `Assets/Scripts/System/NetworkRequest/AuthManager.cs` → `SendAuthorizedRequest()`, `GetDataAsync<T>()`, `GetPublicDataAsync<T>()`, `GetUserDataAsync<T>()`, `SetDataAsync<T>()` 등
- 지원 기능
  - 타입 안전한 API 접근, 404 셀프힐링(기본값 생성→저장→반환)
- 설계 관점
  - 단일 책임 분리(요청 조립/전송/직렬화), OCP/DIP 준수
- 개선 제안
  - 재시도/지수 백오프, 공통 에러 프로토콜, 구조적 로깅 추가.

## 4) 각도 언래핑 + 회전 스무딩(360 경계)
- 요약
  - 359→1 경계에서 역회전/튐 없이 연속 회전을 구현.
- 주요 참조
  - `Assets/Scripts/System/Manager/TimeController.cs` → `Normalize360()`, 보간 로직(`PollTimeCoroutine()` 내부)
- 지원 기능
  - 서버 시계각 기반 태양/환경 연출의 부드러움 보장
- 설계 관점
  - 수학적 정규화와 누적 각도 관리로 외란 제거
- 개선 제안
  - 스무딩 파라미터(응답성/안정성) 노출, 플레이테스트 기반 튜닝.

## 5) 씬 로딩 파이프라인(Photon 연계 + UI 보간 + 애드티브)
- 요약
  - 로딩 단계별 네트워크/데이터/씬을 동기화하고 진행률을 시각적으로 보간.
- 주요 참조
  - `Assets/Scripts/System/LoadingScene/SceneLoader.cs`
- 지원 기능
  - 부드러운 로딩 UX, 다중 단계(데이터/프리팹/Additive) 동기화, Photon 메시지 큐/관심 그룹 제어
- 설계 관점
  - 파이프라인화, 책임 분리, 네트워크 이벤트 제어를 통한 일관성 확보
- 개선 제안
  - 로딩 단계/훅을 데이터화(에셋/스크립터블)하여 모듈 확장 용이화.

## 6) 포스트 프로세싱 제어(Volumes + 커스텀 URP RendererFeature)
- 요약
  - 런타임 상태에 따라 Volume weight 보간 및 커스텀 패스로 연출 확장.
- 주요 참조
  - `Assets/Scripts/System/Manager/PostProcessingManager.cs`
  - `Assets/Scripts/PostProcessing/HurtPostProcessingRendererFeature.cs`
- 지원 기능
  - 피격/달리기/사망 등 상태 기반 연출, 파이프라인 레벨 확장성
- 설계 관점
  - 데이터 주도(Volume) + 엔진 확장(RendererFeature)
- 개선 제안
  - 효과 프리셋/전이 커브를 `ScriptableObject`로 분리, QA 튜닝 루프 단축.

## 7) 리소스 캐시 로더(동기/비동기)
- 요약
  - `Resources.Load(Async)` 위에 캐시를 덧씌워 반복 로드를 방지.
- 주요 참조
  - `Assets/Scripts/System/Manager/ResourceLoader.cs`
- 지원 기능
  - 아이콘/프리팹/사운드 등 반복 로드 비용 절감, 로딩 안정화
- 설계 관점
  - 캐시 키 정책 일관성 중요, 메모리 상한/정리 시점 정의 필요
- 개선 제안
  - LRU 기반 캐시/프리로드 명세 추가, Addressables로의 이전 고려.

## 8) 팩토리 패턴(ItemFactory) + 인벤토리 연계
- 요약
  - 아이템 타입별 생성과 데이터 주입을 표준화하고 인벤토리로 편입.
- 주요 참조
  - `Assets/Scripts/System/Factory/ItemFactory.cs`
- 지원 기능
  - 무기/포션/재료/스킬/스크롤 생성, 랜덤/고정 스탯 분기, 리소스 프리로드
- 설계 관점
  - 팩토리로 생성 책임 분리(OCP), 데이터 테이블과의 경계 명확화
- 개선 제안
  - 생성 레시피를 데이터화하여 신규 아이템 추가 비용 최소화.

## 9) 어댑터 패턴(무기 강화/인챈트)
- 요약
  - 무기 인스턴스와 강화/인챈트 로직을 분리, 교체 가능성 확보.
- 주요 참조
  - `Assets/Scripts/Item/Weapon/Enhance/WeaponEnhancementAdapter.cs`
  - `Assets/Scripts/UI/Inventory/Inventory.cs`(어댑터 상태 영속화)
- 지원 기능
  - 강화 레벨/성공률/재화 차감, 스킬 슬롯/장착/해제
- 설계 관점
  - Adapter + Strategy 조합, SRP/OCP 준수
- 개선 제안
  - 재화 소모/확률 계산을 정책 객체로 분리해 테스트 용이화.

## 10) 반복자(Iterator) 패턴
- 요약
  - 컬렉션 순회를 캡슐화하여 필터/정렬/스택 합산과 결합을 단순화.
- 주요 참조
  - `Assets/Scripts/System/Stuff/Iterator/Aggregate.cs`
  - `Assets/Scripts/System/Stuff/Iterator/Iterator.cs`
  - `Assets/Scripts/UI/Inventory/Inventory.cs` 내 적용
- 지원 기능
  - 인벤토리 컬렉션 순회, 스택 처리, UI 반영
- 설계 관점
  - 컬렉션 노출 최소화, 확장 지점 확보
- 개선 제안
  - LINQ/Span 등과의 혼용 기준 수립, 성능 핫패스에 대한 프로파일링.

## 11) 상태/전략 인터페이스 기반 확장
- 요약
  - 플레이어 상태와 무기 공격 전략을 인터페이스로 분리하여 다형적 확장.
- 주요 참조
  - `Assets/Scripts/Unit/Player/IPlayerState.cs`
  - `Assets/Scripts/Equipment/Weapon/.../IAttackStrategy.cs`
- 지원 기능
  - 런타임 컨텍스트 기반 동작 교체, 신규 상태/전략 추가 용이
- 설계 관점
  - OCP/DIP 준수, 컨텍스트-전략 결합도 관리 필요
- 개선 제안
  - 상태 전이 표(데이터)와 결합, 전략 선택 규칙을 정책 객체화.

## 12) 옵저버(이벤트) 기반 상호작용
- 요약
  - UI/전투/네트워크 반응을 이벤트로 느슨히 결합.
- 주요 참조
  - `Assets/Scripts/UI/Inventory/Inventory.cs` → `OnInventoryItemAdded`, `OnWeaponEquipped`, `OnCurrencyChanged`
  - `Assets/Scripts/System/Manager/Combat/AbilityManager.cs` → 쿨다운 워처 + 이벤트
  - `Assets/Scripts/UI/WindowBase.cs` → `OnWindowClosed`
  - `Assets/Scripts/Photon/PhotonManager.cs` → 룸 입퇴장 이벤트
  - `Assets/Scripts/System/Manager/QuestManager.cs` → `OnTargetQuestChanged`
- 지원 기능
  - 느슨한 결합으로 UI 갱신, 쿨다운 UI, 네트워크 상태 반영
- 설계 관점
  - 이벤트 수명/구독 해제 타이밍 관리 필수
- 개선 제안
  - EventBus/MessagePipe 등으로 표준화, 메모리 누수 방지 패턴 확립.

## 13) 데이터 영속화 & 테이블 로딩
- 요약
  - JSON/바이너리 테이블을 통해 런타임 데이터와 저장 데이터를 관리.
- 주요 참조
  - `Assets/Scripts/UI/Inventory/Inventory.cs`(저장/로드)
  - `Assets/Scripts/System/Table/TableBase.cs`, CSV 리더
- 지원 기능
  - GUID 유지, 타입별 리스트 저장, 로드 시 안정화
- 설계 관점
  - 직렬화 포맷/버전관리 정책 필요
- 개선 제안
  - 버전 필드 도입, 마이그레이션 경로 표준화.

## 14) Photon 네트워킹 제어(메시지 큐/관심 그룹/마스터 스폰)
- 요약
  - 로딩/씬 전환 중 네트워크 이벤트를 제어하고, 마스터 전용 스폰으로 일관성 유지.
- 주요 참조
  - `Assets/Scripts/Photon/PhotonManager.cs`
  - `Assets/Scripts/System/Manager/EnemyManager.cs`
  - `Assets/Scripts/System/LoadingScene/SceneLoader.cs`
- 지원 기능
  - 로딩 안전성, 그룹 분리, 스폰 동기화
- 설계 관점
  - 네트워크 상태 머신과 로딩 파이프라인 결합
- 개선 제안
  - 오류 주입 테스트, 재접속/호스트 마이그레이션 시나리오 점검.

## 15) 대화 시스템(테이블/창 스택/카메라 연계)
- 요약
  - 테이블 기반 대화 흐름을 UI 창/카메라 연출과 결합.
- 주요 참조
  - `Assets/Scripts/System/Manager/DialogueManager.cs`
- 지원 기능
  - 로케일/분기/선택지, 창 스택/카메라 vCam 전환
- 설계 관점
  - 데이터-연출-입력의 분리, 테스트 가능성 우수
- 개선 제안
  - 노드 그래프 시각화/디버거, 로케일 동기화 툴링.

## 16) 카메라 매니저(Cinemachine) 고급 제어
- 요약
  - 멀티 vCam 스위칭, Follow/LookAt 동적 지정, 셰이크/오토센터링.
- 주요 참조
  - `Assets/Scripts/System/Manager/CameraManager.cs`
- 지원 기능
  - 연출 일관성, 입력/상황 기반 전환
- 설계 관점
  - 외부 입력/상태와의 의존성 관리, 전환 규칙 명확화
- 개선 제안
  - 블렌드 테이블 데이터화, 카메라 상태 머신 도입.

## 17) 드롭 시스템(확률/수량/비동기 스폰)
- 요약
  - 적/테이블 기반 확률 추첨→개체 생성→컨테이너 채움.
- 주요 참조
  - `Assets/Scripts/Item/DroppedItem/DropFactory.cs`
- 지원 기능
  - 드롭 밸런싱/연출, 멀티 드롭 처리
- 설계 관점
  - 확률/중복/보장 정책 정의 필요
- 개선 제안
  - 피티 시스템(Pity), 가중치 테이블/시드 관리 추가.

## 18) 퀘스트 그래프/보상/자동 활성화
- 요약
  - 서버 로드→그래프 구성→루트 자동 등록→완료 시 보상/후속 퀘 활성화.
- 주요 참조
  - `Assets/Scripts/System/Manager/QuestManager.cs`
- 지원 기능
  - 흐름 제어/보상 지급(ItemFactory)
- 설계 관점
  - 명시적 상태/전이, 데이터 드리븐 설계
- 개선 제안
  - 조건식/보상식을 스크립터블로 분리, QA 툴 제공.

## 19) UI 창 스택 기본형
- 요약
  - 공통 수명 주기와 닫힘 이벤트로 상호작용 흐름 표준화.
- 주요 참조
  - `Assets/Scripts/UI/WindowBase.cs`(추상)
- 지원 기능
  - 창 전환/중첩/포커스 컨트롤
- 설계 관점
  - 스택/큐/우선순위 정책 명시 필요
- 개선 제안
  - 히스토리/깊이 제한/입력 라우팅 표준화.

## 20) 스크립터블 오브젝트 기반 데이터
- 요약
  - 에디터 친화적 데이터 정의와 런타임 의존성 분리.
- 주요 참조
  - `Assets/Scripts/System/LoadingScene/MapConnection.cs`
  - `Assets/Scripts/Item/ItemData.cs` 등
- 지원 기능
  - 밸런싱/컨텐츠 파이프라인 효율성
- 설계 관점
  - 데이터-코드 분리, 빌드 크기/리소스 경로 관리 필요
- 개선 제안
  - 검증/버전 필드, Addressables 연계.

## 21) 행동트리 베이스 타입
- 요약
  - 직렬화 가능한 노드 베이스와 상태 반환 구조 제공.
- 주요 참조
  - `Assets/Scripts/System/BehaviourTreeNodes/Node.cs`
- 지원 기능
  - AI 노드 확장 기반
- 설계 관점
  - 실행 컨텍스트/블랙보드 설계 필요
- 개선 제안
  - 시각 에디터/디버거 도입.

## 22) IK 기반 발 위치 보정(Animator OnAnimatorIK)
- 요약
  - `OnAnimatorIK` 기반 "접지 순간" 감지로 발소리를 트리거하고, 동시에 좌/우 발의 IK 목표를 지면에 투영해 발 위치/회전을 보정한다.
- 주요 참조
  - `Assets/Scripts/Unit/Player/Player_Movement.cs` → `OnAnimatorIK(int layerIndex)`
- 지원 기능
  - 지형 적응 보행, 경사면 대응, 착지 사운드 일관성 확보
- 구현 포인트
  - 좌/우 발별 접지 순간만 1회 사운드 재생: `leftIK/rightIK` 플래그로 접지 상태 전환을 감지해 디바운스.
  - 표면별 발소리: `SoundManager.PlayFootstepSound(hit.collider.gameObject.layer, isLeft, FootstepWaitTime)`로 레이어 기반 SFX 선택 및 `FootstepWaitTime`으로 스팸 방지.
  - 네트워크 중복 방지: `photonView.IsMine` 검사를 통해 로컬 플레이어에만 사운드/IK 적용.
  - IK 가중치/거리 파라미터: `SetIKPositionWeight/SetIKRotationWeight(0.7f)`, `distanceToGround`, `FootstepType` 레이어 마스크.
- 설계 관점
  - Unity 내장 Animator IK 사용(애니메이션 리깅 제약 미사용). 물리 `Raycast`로 표면 법선을 얻고, `Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal)`로 발 회전을 정렬한다.
  - 네트워크 오너십 검사(`photonView.IsMine`)로 로컬 소유 캐릭터에만 IK를 적용해 불필요한 연산/동기화를 방지한다.
  - 파라미터: `distanceToGround`, `FootstepType` 레이어 마스크, 발별 가중치(`SetIKPositionWeight/SetIKRotationWeight`)를 통해 품질/성능 트레이드오프를 조절한다.
- 개선 제안
  - 발목/무릎 높이 보간 커브 및 가중치 데이터화, 지면 전이 시 프레임 보간 강화.
  - Animation Rigging의 `TwoBoneIKConstraint`로 전환 시, 무릎 폴/타깃 Transform 기반 제어와 조합하여 아티팩트 감소 가능.

---

# 시스템별 교차 참조(샘플)
- 네트워킹/서버
  - `AuthManager`, `PhotonManager`, `EnemyManager`, `SceneLoader`
- UI/UX
  - `WindowBase`, `Inventory`, `DialogueManager`, 로딩 UI
- 게임플레이
  - `ItemFactory`, `WeaponEnhancementAdapter`, `AbilityManager`, `QuestManager`, `DropFactory`
- 렌더링/연출
  - `PostProcessingManager`, `HurtPostProcessingRendererFeature`, `CameraManager`, `TimeController`
- 애니메이션/IK
  - `Player_Movement.OnAnimatorIK` 발 위치 보정, 경사면 회전 정렬, 발소리 트리거
- 인프라/유틸
  - `Singleton`, `ResourceLoader`, Iterator 기반 컬렉션

---

# SOLID/아키텍처 요약
- SRP: 네트워크 요청/직렬화/상태 연출 등 역할 분리 양호. 일부 매니저는 책임이 비대해질 위험 존재.
- OCP: 팩토리/전략/어댑터/인터페이스 도입으로 신규 기능 추가 비용이 낮다.
- LSP: 인터페이스 기반 교체가 빈번, 계약 명세(주석/문서화) 보강 권장.
- ISP: UI/전투/네트워크 등 관심사별 인터페이스 분리. 이벤트 인터페이스/구독 해제 정책 명시 필요.
- DIP: 서비스 로케이터로 런타임 결합도 감소. 테스트/Mock 주입 경로 보강 여지.

---

# 변경 이력
- 2025-09-08: 초기 작성(중복 통합 인벤토리, 시스템 교차 참조, SOLID 요약)
- 2025-09-08 03:06: IK 섹션 추가 및 교차 참조 업데이트
