param (
    [Parameter(Position = 0)]
    [string]$PurchaseDate,

    [Parameter(Position = 1)]
    [int]$WarrantyMonths
)

try {
    # 1. Extract values directly from arguments and perform calculations
    $date = [datetime]::Parse($PurchaseDate)
    $expiryDate = $date.AddMonths($WarrantyMonths)
    
    $remainingDays = ($expiryDate - [datetime]::UtcNow).Days
    $isCovered = $remainingDays -gt 0

    # 2. Construct the response object
    $result = @{
        purchaseDate  = $date.ToString("yyyy-MM-dd")
        expiryDate    = $expiryDate.ToString("yyyy-MM-dd")
        remainingDays = [math]::Max(0, $remainingDays)
        isCovered     = $isCovered
    }

    # 3. Output as JSON so the agent can parse the results natively
    $result | ConvertTo-Json -Compress
}
catch {
    @{ error = $_.Exception.Message } | ConvertTo-Json -Compress
}