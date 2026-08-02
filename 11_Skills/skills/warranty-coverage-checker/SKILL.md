---
name: warranty-coverage-checker
description: Evaluates hardware warranty eligibility based on purchase date and product tier, calculating remaining days and coverage status.
---

# Warranty Coverage Checker Guidelines

Use this skill when a user asks about their product's current warranty status or wants to calculate the cost of purchasing an extended warranty.

## Workflow Execution Steps

### Scenario A: Checking Standard Warranty Status
1. Extract the product category (e.g., Laptops, Headphones).
2. Read `references/product_warranties.json` to find the coverage duration in months.
3. Call the `scripts/calculate_warranty_status.ps1` script, passing the `purchaseDate` and the `warrantyMonths` retrieved in the previous step.
4. Formulate a final response based on the script's output, clearly stating if the product is covered and how many days remain.

### Scenario B: Calculating Extended Warranty Cost
1. Extract the device type (e.g., Laptop, Smartphone) and the requested duration in years.
2. Read `references/extended_warranty_pricing.md` to find the annual price (`PricePerYear`) for the specified device type.
3. Call the `scripts/Calculate-ExtendedWarrantyCost.ps1` script, passing the `PricePerYear` and the `Years`.
4. Formulate a final response based on the script's output, providing the total estimated cost for the extended warranty.
