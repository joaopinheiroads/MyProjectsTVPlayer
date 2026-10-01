IF COL_LENGTH('dbo.Terminal', 'DataAvisoPenultimoDia') IS NULL
BEGIN
    ALTER TABLE dbo.Terminal ADD DataAvisoPenultimoDia datetime NULL;
END
GO
