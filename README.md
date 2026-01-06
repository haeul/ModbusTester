# ModbusTester

Modbus RTU 기반 장비 통신을 테스트하기 위한 **C# WinForms 데스크톱 애플리케이션**입니다.  
수동 송신, 주기 폴링, 프리셋 실행, 매크로 실행, QuickView 모니터링, 로그/레코딩 기능을 제공합니다.

---

## 주요 기능

- **Manual Send**: TX 그리드 기반 수동 Modbus 요청 송신
- **Polling**: 주기적 Read 요청 및 결과 갱신
- **Preset**: TX 설정을 프리셋으로 저장/적용/실행
- **Macro**: 프리셋을 순차 실행하는 매크로 구성 및 실행
- **QuickView**: 선택 레지스터 실시간 모니터링
- **Grid Zoom**: TX/RX 그리드 확대 보기
- **Recording**: 폴링 결과 CSV 기록
- **UI Scaling**: DPI/폰트 기반 UI 스케일링 지원

---

## 프로젝트 구조 개요

본 프로젝트는 **UI / Services / Modbus / Presets / Utils** 계층으로 구성되어 있으며,  
`FormMain`은 역할별 `partial class`로 분리되어 관리됩니다.

---

## UI / Forms

### FormMain.cs
- 메인 UI 초기화 및 전체 상태 연결
- 통신, 그리드, 로깅 등 핵심 구성 요소를 조립
- Modbus 송수신 결과에 따른 화면 업데이트 수행

### FormMain.Lifecycle.cs
- CRC 계산, 수동 전송, 로그/그리드 초기화 처리
- 폼 종료 시 통신 및 기록 리소스 정리

### FormMain.Grid.cs
- TX/RX 그리드 편집 및 삭제 이벤트 처리
- 그리드 확대 창(FormGridZoom) 호출

### FormMain.Polling.cs
- 폴링 시작/중지 및 타이머 기반 주기 통신 제어
- 폴링 결과를 RecordingService로 전달

### FormMain.Preset.cs
- 프리셋 생성/저장/적용 UI 흐름 관리
- 프리셋 실행을 PresetRunner로 위임

### FormMain.QuickView.cs
- RX 그리드에서 QuickView 대상 레지스터 수집
- QuickView 창 생성 및 대상 갱신

### FormMain.Macro.cs
- 매크로 설정 창(FormMacroSettings) 호출 및 활성 상태 관리

### FormMain.Designer.cs
- FormMain의 WinForms 컨트롤 배치 및 UI 초기 구성 정의

---

### FormComSetting.cs
- COM 포트 설정 UI
- Master / Slave 모드에 따라 메인 폼 실행
- 포트 및 슬레이브 리소스 생성·해제 흐름 관리

### FormComSetting.Designer.cs
- COM 설정 폼 UI 구성 정의

---

### FormQuickView.cs
- 선택된 레지스터를 ChannelRowControl로 구성
- RegisterCache 변경 이벤트에 따라 표시 값 갱신
- 표시 폰트 로드 및 레이아웃 자동 조정

### FormQuickView.Designer.cs
- QuickView 폼 UI 구성 정의

---

### FormGridZoom.cs
- DataGridView를 확대 표시하는 전용 창
- 원본 그리드와 컬럼/행 동기화
- 확대 폰트 및 행 높이 조절 UI 제공

### FormGridZoom.Designer.cs
- Grid Zoom 폼 UI 구성 정의

---

### FormMacroSettings.cs
- 매크로 정의 및 단계 편집 UI
- 매크로 실행 인스턴스의 런타임 상태 제어

### FormMacroSettings.Designer.cs
- 매크로 설정 폼 UI 구성 정의

---

## Controls

### ChannelRowControl.cs
- QuickView에서 사용하는 레지스터 표시용 UserControl
- DEC / HEX / BIT 값 표시 및 갱신 처리

### ChannelRowControl.Designer.cs
- ChannelRowControl UI 구성 정의

### HexNumericUpDown.cs
- 16진수 입력/표시를 지원하는 NumericUpDown 파생 컨트롤

---

## Services

### ModbusMasterService.cs
- Modbus RTU 요청/응답을 고수준 API로 제공
- Read / Write 요청 처리 로직 집약

### SerialModbusClient.cs
- SerialPort 기반 RTU 프레임 송수신 전담
- 요청 전송 및 응답 수신 처리

### ModbusPoller.cs
- 주기적인 Modbus Read 폴링 수행
- 폴링 결과 DTO 생성 및 반환

### RecordingService.cs
- 폴링된 레지스터 값을 CSV 파일로 기록
- 녹화 세션 시작/중지 관리

### UiLogger.cs
- RichTextBox 기반 TX/RX 로그 출력
- 송수신 방향별 색상 구분 처리

---

## Modbus

### ModbusRtu.cs
- Modbus RTU 프레임 생성 및 파싱
- CRC 계산 유틸 제공

### ModbusSlave.cs
- 간단한 Modbus RTU 슬레이브 에뮬레이터
- 요청 수신 후 레지스터 읽기/쓰기 응답 생성

---

## Presets / Macros

### FunctionPreset.cs
- 프리셋 데이터 구조 정의 (메타 정보 + TX 행)

### FunctionPresetManager.cs
- 프리셋 JSON 로드/저장 및 관리

### PresetRunner.cs
- 프리셋 실행 로직 캡슐화
- 실행 결과를 UI 콜백으로 전달

### MacroDefinition.cs
- 매크로 및 매크로 단계 데이터 구조 정의

### MacroManager.cs
- 매크로 JSON 로드/저장 및 관리

---

## Utils / Core

### RegisterGridController.cs
- TX/RX DataGridView 초기화 및 편집 동기화
- 값 복사, 스냅샷/복원 등 그리드 유틸 로직 집약

### RegisterCache.cs
- 레지스터 값을 메모리에 캐시
- 값 변경 이벤트 발행

### ComPortConfig.cs
- COM 포트 설정 값을 담는 구성 모델

### FontScaler.cs
- 컨트롤 트리 전체의 폰트 크기 일괄 조정

### LayoutScaler.cs
- 폼 리사이즈 시 컨트롤 위치/크기/폰트 비율 스케일링

### HexExtensions.cs
- HEX / 비트 문자열 변환 및 파싱 확장 메서드

### AppVersion.cs
- 실행 어셈블리 버전 문자열 추출 및 정규화

---

## 실행 환경

- Windows OS
- .NET (WinForms)
- SerialPort 기반 Modbus RTU 장비 또는 에뮬레이터

---

## 주의 사항

- 본 도구는 **테스트 및 개발 목적**으로 설계되었습니다.
- 실제 장비 연결 시 통신 파라미터 및 Write 동작에 주의하세요.
