-- Onboarding progress report per enterprise customer
-- Tracks milestones: kickoff → UAT → go-live
-- Used in Tipalti and DocuSign implementation dashboards

WITH MilestoneProgress AS (
    SELECT
        c.CustomerID,
        c.CompanyName,
        c.ContractValue,
        c.AssignedManager,
        m.MilestoneName,
        m.PlannedDate,
        m.CompletedDate,
        CASE
            WHEN m.CompletedDate IS NOT NULL
                THEN 'Completed'
            WHEN m.PlannedDate < GETDATE()
                THEN 'Overdue'
            ELSE 'Pending'
        END AS Status,
        DATEDIFF(day,
            m.PlannedDate,
            ISNULL(m.CompletedDate, GETDATE())) AS SlippageDays
    FROM Customers c
    INNER JOIN OnboardingMilestones m
        ON c.CustomerID = m.CustomerID
    WHERE c.Status = 'Active'
),
Summary AS (
    SELECT
        CustomerID,
        CompanyName,
        ContractValue,
        AssignedManager,
        COUNT(*) AS TotalMilestones,
        SUM(CASE WHEN Status = 'Completed'
            THEN 1 ELSE 0 END) AS CompletedCount,
        MAX(CASE WHEN Status = 'Overdue'
            THEN SlippageDays ELSE 0 END) AS MaxSlippage
    FROM MilestoneProgress
    GROUP BY
        CustomerID, CompanyName,
        ContractValue, AssignedManager
)
SELECT
    CompanyName,
    AssignedManager,
    FORMAT(ContractValue, 'C0') AS ContractValue,
    CONCAT(CompletedCount, ' / ', TotalMilestones)
        AS MilestoneProgress,
    CAST(CompletedCount * 100.0
        / TotalMilestones AS INT) AS PctComplete,
    MaxSlippage AS MaxSlippageDays
FROM Summary
ORDER BY ContractValue DESC, PctComplete ASC;
