# ================================
# TIMER START
# ================================
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

# ================================
# CONFIGURACIÓN
# ================================
$root = $PSScriptRoot

$parentFolder = Join-Path -Path $root -ChildPath ".."
$csvPath = Join-Path -Path $parentFolder -ChildPath "file_base\laminas.csv"
$csvPath = (Resolve-Path $csvPath).Path

# Forzar UTF8 para respetar tildes
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# ================================
# MAPA DE GRUPOS (REAL 2026)
# ================================

$groupMap = @{
    "MEX"="A"; "KOR"="A"; "RSA"="A"; "CZE"="A";
    "CAN"="B"; "SUI"="B"; "QAT"="B"; "BIH"="B";
    "BRA"="C"; "MAR"="C"; "HAI"="C"; "SCO"="C";
    "USA"="D"; "PAR"="D"; "AUS"="D"; "TUR"="D";
    "GER"="E"; "CUW"="E"; "CIV"="E"; "ECU"="E";
    "NED"="F"; "JPN"="F"; "SWE"="F"; "TUN"="F";
    "BEL"="G"; "EGY"="G"; "IRN"="G"; "NZL"="G";
    "ESP"="H"; "CPV"="H"; "KSA"="H"; "URU"="H";
    "FRA"="I"; "SEN"="I"; "IRQ"="I"; "NOR"="I";
    "ARG"="J"; "ALG"="J"; "AUT"="J"; "JOR"="J";
    "POR"="K"; "COD"="K"; "UZB"="K"; "COL"="K";
    "ENG"="L"; "CRO"="L"; "GHA"="L"; "PAN"="L";
}

# ================================
# MAPA DE BANDERAS (ISO alpha-2)
# ================================

$flagMap = @{
    "MEX"="mx"; "KOR"="kr"; "RSA"="za"; "CZE"="cz";
    "CAN"="ca"; "SUI"="ch"; "QAT"="qa"; "BIH"="ba";
    "BRA"="br"; "MAR"="ma"; "HAI"="ht"; "SCO"="gb-sct";
    "USA"="us"; "PAR"="py"; "AUS"="au"; "TUR"="tr";
    "GER"="de"; "CUW"="cw"; "CIV"="ci"; "ECU"="ec";
    "NED"="nl"; "JPN"="jp"; "SWE"="se"; "TUN"="tn";
    "BEL"="be"; "EGY"="eg"; "IRN"="ir"; "NZL"="nz";
    "ESP"="es"; "CPV"="cv"; "KSA"="sa"; "URU"="uy";
    "FRA"="fr"; "SEN"="sn"; "IRQ"="iq"; "NOR"="no";
    "ARG"="ar"; "ALG"="dz"; "AUT"="at"; "JOR"="jo";
    "POR"="pt"; "COD"="cd"; "UZB"="uz"; "COL"="co";
    "ENG"="gb-eng"; "CRO"="hr"; "GHA"="gh"; "PAN"="pa";
}

# ================================
# LEER CSV
# ================================

# $data = Import-Csv -Path $csvPath -Delimiter ';' -Encoding UTF8
# $content = Get-Content $csvPath -Raw -Encoding UTF8
# $data = $content | ConvertFrom-Csv -Delimiter ';'

$content = Get-Content $csvPath -Raw -Encoding Default
$data = $content | ConvertFrom-Csv -Delimiter ';'

# ================================
# PROCESAMIENTO
# ================================

$fcw = @{
    group = "FCW"
    stickers = @()
}

$countries = @{}
$displayOrder = 1

foreach ($row in $data) {

    $id = $row.Id.Trim()
    $title = $row.Titulo.Trim()
    $section = $row.Seccion.Trim()

    # ============================
    # FCW
    # ============================
    if ($id -like "FWC*" -or $id.ToString() -in "0", "00") {
        $fcw.stickers += @{
            stickerCode = $id
            displayName = $title
            displayOrder = [int]($id -replace 'FWC','')
        }
        continue
    }

    # ============================
    # PAISES
    # ============================
    if ($id.Length -ge 4) {

        $team = $id.Substring(0,3)
        $number = [int]($id.Substring(3))

        if (-not $countries.ContainsKey($team)) {
            $countries[$team] = @{
                code = $team
                name = $section
                group = $groupMap[$team]
                flagCode = if ($flagMap.ContainsKey($team)) { $flagMap[$team] } else { $team.ToLower() }
                displayOrder = $displayOrder
                stickers = @()
            }
            $displayOrder++
        }

        # ========================
        # TIPO DE LÁMINA
        # ========================

        if ($number -eq 1) {
            $type = "escudo"
        }
        elseif ($number -eq 13) {
            $type = "equipo"
        }
        else {
            $type = "jugador"
        }

        # ========================
        # NOMBRE
        # ========================

        if ($type -eq "jugador") {
            # $playerIndex = $number - 2
            $displayName = $title
        }
        elseif ($type -eq "escudo") {
            $displayName = "Escudo $section"
        }
        elseif ($type -eq "equipo") {
            $displayName = "Equipo $section"
        }
        else {
            $displayName = $title
        }

        $countries[$team].stickers += @{
            stickerCode = $id
            displayName = $displayName
            type = $type
            displayOrder = $number
            birthday = ""
			height = ""
			weight = ""
			team = ""
        }
    }
}

# ================================
# JSON FINAL
# ================================

$result = @{
    fcw = $fcw
    countries = $countries.Values
}

$json = $result | ConvertTo-Json -Depth 10

# ================================
# COPIAR AL PORTAPAPELES
# ================================

$json | Set-Clipboard

Write-Host "✅ JSON generado desde CSV y copiado al portapapeles"

# ================================
# TIMER STOP
# ================================
$stopwatch.Stop()

$elapsed = $stopwatch.Elapsed

Write-Host "Tiempo de ejecución:" -ForegroundColor Green
Write-Host ("   {0:mm\:ss\.fff}" -f $elapsed) -ForegroundColor Green
Write-Host ("   Total segundos: {0}" -f [math]::Round($elapsed.TotalSeconds, 3)) -ForegroundColor Green