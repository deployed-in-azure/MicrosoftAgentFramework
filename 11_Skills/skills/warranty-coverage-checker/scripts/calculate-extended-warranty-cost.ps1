param (
    [Parameter(Position = 0)]
    [decimal]$PricePerYear,

    [Parameter(Position = 1)]
    [int]$Years
)

try {
    $totalCost = $PricePerYear * $Years
    
    $result = @{
        totalCost = [math]::Round($totalCost, 2)
        currency = "USD"
    }

    $result | ConvertTo-Json -Compress
}
catch {
    @{ error = $_.Exception.Message } | ConvertTo-Json -Compress
}