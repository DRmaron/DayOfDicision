# Generates Assets/Resources/ScenarioPDF/*.asset from templates (Unity YAML)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not (Test-Path (Join-Path $root "Assets"))) {
    $root = Join-Path $PSScriptRoot ".."
}
$outDir = Join-Path $root "Assets\Resources\ScenarioPDF"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$scriptGuid = "986793f54c56f804fa9c63dc989f7757"

$names = @(
    "Evac_01","Evac_02","Evac_03","Evac_04","Evac_05","Evac_06","Evac_Result",
    "Shelter_01","Shelter_02","Shelter_03","Shelter_04","Shelter_05","Shelter_06",
    "Shelter_07","Shelter_08","Shelter_09","Shelter_10","Shelter_11","Ending"
)

$guids = @{}
for ($i = 0; $i -lt $names.Count; $i++) {
    $guids[$names[$i]] = ("f100000000000000000000000000{0:x4}" -f ($i + 1))
}

function Ref([string]$name) {
    $g = $guids[$name]
    return "{fileID: 11400000, guid: $g, type: 2}"
}

function Write-Meta([string]$path, [string]$guid) {
    @"
fileFormatVersion: 2
guid: $guid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@ | Set-Content -Path $path -Encoding UTF8
}

function Write-Asset([string]$fileName, [string]$eventId, [int]$phase, [string]$text, [string]$nextA, [string]$nextB, [hashtable]$choiceA, [hashtable]$choiceB) {
    $path = Join-Path $outDir "$fileName.asset"
    $na = if ($nextA) { Ref $nextA } else { "{fileID: 0}" }
    $nb = if ($nextB) { Ref $nextB } else { "{fileID: 0}" }
    $yaml = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $scriptGuid, type: 3}
  m_Name: $fileName
  m_EditorClassIdentifier: 
  eventId: $eventId
  phase: $phase
  priority: 0
  eventText: "$($text -replace '"','\"')"
  requiredFlags: []
  consumeOnce: 0
  choiceA:
    label: "$($choiceA.label -replace '"','\"')"
    resultText: 
    hpDelta: $($choiceA.hp)
    hungerDelta: $($choiceA.hunger)
    sanDelta: $($choiceA.san)
    suppliesDelta: $($choiceA.supplies)
    waterDelta: $($choiceA.water)
    hygieneDelta: $($choiceA.hygiene)
    trustDelta: $($choiceA.trust)
    coopDelta: $($choiceA.coop)
    suppliesCost: 0
    requiredFlags: []
    addFlags: [$($choiceA.flags)]
    removeFlags: []
    switchPhaseAfterChoice: $($choiceA.switchPhase)
    nextPhase: $($choiceA.nextPhase)
    resetRunAfterChoice: $($choiceA.resetRun)
  choiceB:
    label: "$($choiceB.label -replace '"','\"')"
    resultText: 
    hpDelta: $($choiceB.hp)
    hungerDelta: $($choiceB.hunger)
    sanDelta: $($choiceB.san)
    suppliesDelta: $($choiceB.supplies)
    waterDelta: $($choiceB.water)
    hygieneDelta: $($choiceB.hygiene)
    trustDelta: $($choiceB.trust)
    coopDelta: $($choiceB.coop)
    suppliesCost: 0
    requiredFlags: [$($choiceB.reqFlags)]
    addFlags: [$($choiceB.flags)]
    removeFlags: []
    switchPhaseAfterChoice: $($choiceB.switchPhase)
    nextPhase: $($choiceB.nextPhase)
    resetRunAfterChoice: $($choiceB.resetRun)
  nextEventAfterChoiceA: $na
  nextEventAfterChoiceB: $nb
"@
    Set-Content -Path $path -Value $yaml -Encoding UTF8
    Write-Meta "$path.meta" $guids[$fileName]
}

$emptyB = @{ label="-"; hp=0;hunger=0;san=0;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";reqFlags='"__never__"';switchPhase=0;nextPhase=0;resetRun=0 }

# Evac chain (abbreviated labels - full text in GameManager GetEventText for special ids)
Write-Asset "Evac_01" "EVAC_01" 0 "Evac01" "Evac_02" "Evac_02" @{label="A：水と食料";hp=-1;hunger=2;san=0;supplies=1;water=20;hygiene=0;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=0} @{label="B：救急とライト";hp=0;hunger=0;san=1;supplies=0;water=0;hygiene=10;trust=0;coop=1;flags="";switchPhase=0;nextPhase=0;resetRun=0}
Write-Asset "Evac_02" "EVAC_02" 0 "Evac02" "Evac_03" "Evac_03" @{label="A：障害物をどかす";hp=-1;hunger=0;san=1;supplies=0;water=0;hygiene=0;trust=0;coop=1;flags='"helpedObstacle"';switchPhase=0;nextPhase=0;resetRun=0} @{label="B：別の道";hp=0;hunger=0;san=-1;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=0}
Write-Asset "Evac_03" "EVAC_03" 0 "Evac03" "Evac_04" "Evac_04" @{label="A：物資を探す";hp=-1;hunger=0;san=0;supplies=2;water=10;hygiene=5;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=0} @{label="B：入らない";hp=0;hunger=0;san=0;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=0}
Write-Asset "Evac_04" "EVAC_04" 0 "Evac04" "Evac_05" "Evac_05" @{label="A：連れていく";hp=-1;hunger=0;san=2;supplies=0;water=-5;hygiene=0;trust=0;coop=2;flags='"helpedChild"';switchPhase=0;nextPhase=0;resetRun=0} @{label="B：方向だけ";hp=0;hunger=0;san=-1;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=0}
Write-Asset "Evac_05" "EVAC_05" 0 "Evac05" "Evac_06" "Evac_06" @{label="A：水を分ける";hp=0;hunger=0;san=1;supplies=0;water=-10;hygiene=0;trust=0;coop=1;flags='"sharedWater"';switchPhase=0;nextPhase=0;resetRun=0} @{label="B：残す";hp=0;hunger=0;san=-1;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=0}
Write-Asset "Evac_06" "EVAC_06" 0 "Evac06" "Evac_Result" "Evac_Result" @{label="A：荷物を持つ";hp=-1;hunger=0;san=1;supplies=0;water=0;hygiene=0;trust=0;coop=1;flags="";switchPhase=0;nextPhase=0;resetRun=0} @{label="B：自分のペース";hp=0;hunger=0;san=0;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=0}
Write-Asset "Evac_Result" "EVAC_RESULT" 0 "EvacResult" "Shelter_01" "Shelter_01" @{label="避難所へ入る";hp=0;hunger=0;san=0;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";switchPhase=1;nextPhase=1;resetRun=0} $emptyB

# Shelter
for ($n = 1; $n -le 11; $n++) {
    $id = "{0:D2}" -f $n
    $file = "Shelter_$id"
    $next = if ($n -lt 11) { "Shelter_{0:D2}" -f ($n+1) } else { "Ending" }
    $ca = @{label="A";hp=0;hunger=0;san=0;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=0}
    $cb = @{label="B";hp=0;hunger=0;san=0;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=0}
    if ($n -eq 4) {
        $ca.flags = '"scamVictim"'
        $ca.san = -2
        $cb.flags = '"scamAvoided"'
        $cb.trust = 10
        $cb.san = 1
    }
    Write-Asset $file "SH_$id" 1 "Shelter$n" $next $next $ca $cb
}

Write-Asset "Ending" "ENDING" 1 "Ending" $null $null @{label="リスタート";hp=0;hunger=0;san=0;supplies=0;water=0;hygiene=0;trust=0;coop=0;flags="";switchPhase=0;nextPhase=0;resetRun=1} $emptyB

Write-Host "Generated PDF scenario assets in $outDir"
