param(
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,
    [Parameter(Mandatory = $true)]
    [string]$ProjectCopyPath,
    [Parameter(Mandatory = $true)]
    [string]$PreviewPdfPath,
    [Parameter(Mandatory = $true)]
    [string]$PreviewPngPath
)

$ErrorActionPreference = 'Stop'

$rows = @(
    @('StageDuration', 'float', '480', 'Seconds before the Reaper boss appears.', 'Stage'),
    @('BaseSpawnInterval', 'float', '0.85', 'Initial seconds between enemy spawn batches.', 'Stage'),
    @('MaxEnemies', 'int', '180', 'Maximum simultaneous normal enemies.', 'Stage'),
    @('ChestInterval', 'float', '60', 'Seconds between chest spawns.', 'Stage'),
    @('FinalWaveTime', 'float', '450', 'Seconds when the large final wave appears.', 'Stage'),
    @('FinalWaveCount', 'int', '28', 'Enemies spawned by the large final wave.', 'Stage'),
    @('HealthDropChance', 'float', '0.02', 'Health drop probability from 0 to 1.', 'Drops'),
    @('MagnetDropChance', 'float', '0.0075', 'Magnet drop probability from 0 to 1.', 'Drops'),
    @('MagnetDuration', 'float', '6', 'Magnet effect duration in seconds.', 'Drops'),
    @('EnemyHealthScaleAtEnd', 'float', '0.65', 'Additional enemy health ratio at stage end.', 'Stage'),
    @('BaseMaxHealth', 'float', '100', 'Player health before character multipliers.', 'Player'),
    @('BaseMoveSpeed', 'float', '4.2', 'Player movement speed before character multipliers.', 'Player'),
    @('BasePickupRadius', 'float', '1.8', 'Player pickup radius before passive upgrades.', 'Player'),

    @('ScytheBaseCooldown', 'float', '1.15', 'Scythe base attack interval in seconds.', 'Weapon: Scythe'),
    @('ScytheCooldownPerLevel', 'float', '0.07', 'Cooldown reduction per scythe level.', 'Weapon: Scythe'),
    @('ScytheMinCooldown', 'float', '0.28', 'Minimum scythe attack interval.', 'Weapon: Scythe'),
    @('ScytheBaseDamage', 'float', '14', 'Scythe base damage.', 'Weapon: Scythe'),
    @('ScytheDamagePerLevel', 'float', '5', 'Scythe damage gained per level.', 'Weapon: Scythe'),
    @('ScytheBaseRadius', 'float', '1.65', 'Scythe base hit radius.', 'Weapon: Scythe'),
    @('ScytheRadiusPerLevel', 'float', '0.18', 'Scythe radius gained per level.', 'Weapon: Scythe'),
    @('SeedBaseCooldown', 'float', '0.88', 'Seed gun base firing interval.', 'Weapon: Seed Gun'),
    @('SeedCooldownPerLevel', 'float', '0.09', 'Seed gun cooldown reduction per level.', 'Weapon: Seed Gun'),
    @('SeedMinCooldown', 'float', '0.18', 'Minimum seed gun firing interval.', 'Weapon: Seed Gun'),
    @('SeedBaseDamage', 'float', '9', 'Seed projectile base damage.', 'Weapon: Seed Gun'),
    @('SeedDamagePerLevel', 'float', '3', 'Seed projectile damage gained per level.', 'Weapon: Seed Gun'),
    @('SeedProjectileSpeed', 'float', '9', 'Seed projectile travel speed.', 'Weapon: Seed Gun'),
    @('OrbitBaseDamage', 'float', '18', 'Orbiting pickaxe base damage.', 'Weapon: Pickaxe'),
    @('OrbitDamagePerLevel', 'float', '2.5', 'Orbiting pickaxe damage gained per level.', 'Weapon: Pickaxe'),
    @('OrbitBaseSpeed', 'float', '105', 'Orbit rotation speed in degrees per second.', 'Weapon: Pickaxe'),
    @('OrbitSpeedPerLevel', 'float', '14', 'Orbit rotation speed gained per level.', 'Weapon: Pickaxe'),
    @('OrbitBaseRadius', 'float', '1.55', 'Base distance from the player.', 'Weapon: Pickaxe'),
    @('OrbitRadiusPerLevel', 'float', '0.08', 'Orbit distance gained per level.', 'Weapon: Pickaxe'),
    @('OrbitBaseHitRadius', 'float', '0.48', 'Base pickaxe collision radius.', 'Weapon: Pickaxe'),
    @('OrbitHitRadiusPerLevel', 'float', '0.04', 'Collision radius gained per level.', 'Weapon: Pickaxe'),
    @('OrbitBaseTick', 'float', '0.42', 'Base seconds between orbit damage checks.', 'Weapon: Pickaxe'),
    @('OrbitTickPerLevel', 'float', '0.035', 'Damage-check interval reduction per level.', 'Weapon: Pickaxe'),
    @('OrbitMinTick', 'float', '0.15', 'Minimum orbit damage-check interval.', 'Weapon: Pickaxe'),

    @('Farmer0DamageMultiplier', 'float', '1.1', 'Balanced farmer damage multiplier.', 'Character 0'),
    @('Farmer0MoveSpeedMultiplier', 'float', '1', 'Balanced farmer move-speed multiplier.', 'Character 0'),
    @('Farmer0CooldownMultiplier', 'float', '1', 'Balanced farmer cooldown multiplier.', 'Character 0'),
    @('Farmer0MaxHealthMultiplier', 'float', '1', 'Balanced farmer health multiplier.', 'Character 0'),
    @('Farmer1DamageMultiplier', 'float', '1', 'Sprinter damage multiplier.', 'Character 1'),
    @('Farmer1MoveSpeedMultiplier', 'float', '1.15', 'Sprinter move-speed multiplier.', 'Character 1'),
    @('Farmer1CooldownMultiplier', 'float', '1', 'Sprinter cooldown multiplier.', 'Character 1'),
    @('Farmer1MaxHealthMultiplier', 'float', '1', 'Sprinter health multiplier.', 'Character 1'),
    @('Farmer2DamageMultiplier', 'float', '1', 'Quickshot damage multiplier.', 'Character 2'),
    @('Farmer2MoveSpeedMultiplier', 'float', '1', 'Quickshot move-speed multiplier.', 'Character 2'),
    @('Farmer2CooldownMultiplier', 'float', '0.9', 'Quickshot cooldown multiplier.', 'Character 2'),
    @('Farmer2MaxHealthMultiplier', 'float', '1', 'Quickshot health multiplier.', 'Character 2'),
    @('Farmer3DamageMultiplier', 'float', '1', 'Survivor damage multiplier.', 'Character 3'),
    @('Farmer3MoveSpeedMultiplier', 'float', '1', 'Survivor move-speed multiplier.', 'Character 3'),
    @('Farmer3CooldownMultiplier', 'float', '1', 'Survivor cooldown multiplier.', 'Character 3'),
    @('Farmer3MaxHealthMultiplier', 'float', '1.25', 'Survivor health multiplier.', 'Character 3'),

    @('ZombieMaxHealth', 'float', '18', 'Zombie base health.', 'Enemy: Zombie'),
    @('ZombieMoveSpeed', 'float', '1.45', 'Zombie movement speed.', 'Enemy: Zombie'),
    @('ZombieTouchDamage', 'float', '8', 'Zombie contact damage.', 'Enemy: Zombie'),
    @('ZombieExperience', 'int', '1', 'Experience dropped by Zombie.', 'Enemy: Zombie'),
    @('ZombieAttackCooldown', 'float', '2.5', 'Zombie attack cooldown.', 'Enemy: Zombie'),
    @('RunnerMaxHealth', 'float', '14', 'Runner base health.', 'Enemy: Runner'),
    @('RunnerMoveSpeed', 'float', '2.45', 'Runner movement speed.', 'Enemy: Runner'),
    @('RunnerTouchDamage', 'float', '7', 'Runner contact damage.', 'Enemy: Runner'),
    @('RunnerExperience', 'int', '2', 'Experience dropped by Runner.', 'Enemy: Runner'),
    @('RunnerAttackCooldown', 'float', '2.2', 'Runner attack cooldown.', 'Enemy: Runner'),
    @('SkeletonMaxHealth', 'float', '48', 'Skeleton base health.', 'Enemy: Skeleton'),
    @('SkeletonMoveSpeed', 'float', '1.05', 'Skeleton movement speed.', 'Enemy: Skeleton'),
    @('SkeletonTouchDamage', 'float', '13', 'Skeleton contact damage.', 'Enemy: Skeleton'),
    @('SkeletonExperience', 'int', '4', 'Experience dropped by Skeleton.', 'Enemy: Skeleton'),
    @('SkeletonAttackCooldown', 'float', '2.5', 'Skeleton attack cooldown.', 'Enemy: Skeleton'),
    @('MummyMaxHealth', 'float', '34', 'Mummy base health.', 'Enemy: Mummy'),
    @('MummyMoveSpeed', 'float', '1.2', 'Mummy movement speed.', 'Enemy: Mummy'),
    @('MummyTouchDamage', 'float', '10', 'Mummy contact damage.', 'Enemy: Mummy'),
    @('MummyExperience', 'int', '4', 'Experience dropped by Mummy.', 'Enemy: Mummy'),
    @('MummyAttackCooldown', 'float', '2.35', 'Mummy ranged attack cooldown.', 'Enemy: Mummy'),
    @('ReaperMaxHealth', 'float', '950', 'Reaper boss base health.', 'Enemy: Reaper'),
    @('ReaperMoveSpeed', 'float', '1.15', 'Reaper boss movement speed.', 'Enemy: Reaper'),
    @('ReaperTouchDamage', 'float', '22', 'Reaper boss contact damage.', 'Enemy: Reaper'),
    @('ReaperExperience', 'int', '80', 'Experience dropped by Reaper.', 'Enemy: Reaper'),
    @('ReaperAttackCooldown', 'float', '1.8', 'Reaper boss attack cooldown.', 'Enemy: Reaper')
)

$outputDirectory = Split-Path -Parent $OutputPath
$projectDirectory = Split-Path -Parent $ProjectCopyPath
$previewDirectory = Split-Path -Parent $PreviewPdfPath
$previewPngDirectory = Split-Path -Parent $PreviewPngPath
New-Item -ItemType Directory -Force -Path $outputDirectory, $projectDirectory, $previewDirectory, $previewPngDirectory | Out-Null

$excel = $null
$workbook = $null
$sheet = $null
try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    $workbook = $excel.Workbooks.Add()
    $sheet = $workbook.Worksheets.Item(1)
    $sheet.Name = 'FarmBalance'

    $headers = @('Key', 'Type', 'Value', 'Description', 'Section')
    for ($column = 1; $column -le $headers.Count; $column++) {
        $sheet.Cells.Item(1, $column).Value2 = $headers[$column - 1]
        $sheet.Cells.Item(2, $column).Value2 = 'string'
    }

    for ($index = 0; $index -lt $rows.Count; $index++) {
        $excelRow = $index + 3
        for ($column = 1; $column -le 5; $column++) {
            $sheet.Cells.Item($excelRow, $column).Value2 = [string]$rows[$index][$column - 1]
        }
    }

    $lastRow = $rows.Count + 2
    $used = $sheet.Range("A1:E$lastRow")
    $used.Font.Name = 'Aptos'
    $used.Font.Size = 10
    $used.VerticalAlignment = -4108
    $sheet.Range("A1:E1").Interior.Color = 0x754C19
    $sheet.Range("A1:E1").Font.Color = 0xFFFFFF
    $sheet.Range("A1:E1").Font.Bold = $true
    $sheet.Range("A1:E1").HorizontalAlignment = -4108
    $sheet.Range("A2:E2").Interior.Color = 0xEADFCB
    $sheet.Range("A2:E2").Font.Color = 0x6B5A3A
    $sheet.Range("A2:E2").Font.Italic = $true
    $sheet.Range("C3:C$lastRow").Interior.Color = 0xD9F2FF
    $sheet.Range("C3:C$lastRow").NumberFormat = '@'
    $sheet.Range("A3:A$lastRow").Font.Bold = $true
    $sheet.Range("D3:D$lastRow").Font.Color = 0x666666
    $sheet.Range("E3:E$lastRow").Font.Color = 0x806000

    $sheet.Columns.Item('A').ColumnWidth = 31
    $sheet.Columns.Item('B').ColumnWidth = 11
    $sheet.Columns.Item('C').ColumnWidth = 14
    $sheet.Columns.Item('D').ColumnWidth = 48
    $sheet.Columns.Item('E').ColumnWidth = 20
    $sheet.Rows.Item(1).RowHeight = 24
    $sheet.Rows.Item(2).RowHeight = 20
    $sheet.Range("A1:E$lastRow").Borders.Item(9).LineStyle = 1
    $sheet.Range("A1:E$lastRow").Borders.Item(9).Color = 0xD9D9D9
    $sheet.Range("A1:E$lastRow").Borders.Item(10).LineStyle = 1
    $sheet.Range("A1:E$lastRow").Borders.Item(10).Color = 0xD9D9D9
    $sheet.Range("A1:E1").AutoFilter() | Out-Null

    $sheet.Activate()
    $excel.ActiveWindow.DisplayGridlines = $false
    $excel.ActiveWindow.SplitRow = 2
    $excel.ActiveWindow.FreezePanes = $true
    $excel.ActiveWindow.Zoom = 90

    $sheet.PageSetup.Orientation = 2
    $sheet.PageSetup.Zoom = $false
    $sheet.PageSetup.FitToPagesWide = 1
    $sheet.PageSetup.FitToPagesTall = $false
    $sheet.PageSetup.PrintTitleRows = '$1:$2'
    $sheet.PageSetup.LeftMargin = $excel.InchesToPoints(0.35)
    $sheet.PageSetup.RightMargin = $excel.InchesToPoints(0.35)
    $sheet.PageSetup.TopMargin = $excel.InchesToPoints(0.5)
    $sheet.PageSetup.BottomMargin = $excel.InchesToPoints(0.5)

    if (Test-Path -LiteralPath $OutputPath) { Remove-Item -LiteralPath $OutputPath -Force }
    if (Test-Path -LiteralPath $ProjectCopyPath) { Remove-Item -LiteralPath $ProjectCopyPath -Force }
    if (Test-Path -LiteralPath $PreviewPdfPath) { Remove-Item -LiteralPath $PreviewPdfPath -Force }
    if (Test-Path -LiteralPath $PreviewPngPath) { Remove-Item -LiteralPath $PreviewPngPath -Force }
    $workbook.SaveAs($OutputPath, 51)
    $sheet.ExportAsFixedFormat(0, $PreviewPdfPath)

    $sheet.Range('A1:E32').CopyPicture(1, 2) | Out-Null
    Start-Sleep -Milliseconds 400
    $chartObject = $sheet.ChartObjects().Add(10, 10, 1280, 900)
    $chartObject.Activate() | Out-Null
    $chartObject.Chart.Paste() | Out-Null
    Start-Sleep -Milliseconds 400
    $chartObject.Chart.Export($PreviewPngPath, 'PNG') | Out-Null
    $chartObject.Delete()

    $workbook.SaveCopyAs($ProjectCopyPath)
}
finally {
    if ($workbook -ne $null) { $workbook.Close($false) }
    if ($excel -ne $null) { $excel.Quit() }
    if ($sheet -ne $null) { [void][Runtime.InteropServices.Marshal]::ReleaseComObject($sheet) }
    if ($workbook -ne $null) { [void][Runtime.InteropServices.Marshal]::ReleaseComObject($workbook) }
    if ($excel -ne $null) { [void][Runtime.InteropServices.Marshal]::ReleaseComObject($excel) }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}

Write-Output "Created $OutputPath"
Write-Output "Copied to $ProjectCopyPath"
Write-Output "Previewed at $PreviewPdfPath"
Write-Output "Image preview at $PreviewPngPath"
