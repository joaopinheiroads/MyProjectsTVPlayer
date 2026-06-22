-- Verificar grupos adicionais disponíveis para a empresa 1301
SELECT TOP 10 
    ga.ID as GrupoAdicionalID,
    ga.Nome,
    ga.EmpresaID,
    ga.Ativo
FROM GrupoAdicional ga 
WHERE ga.EmpresaID = 1301 AND ga.Ativo = 1;

-- Verificar se o produto "teste 8" (ID: 3383) tem grupos adicionais associados
SELECT 
    p.ID as ProdutoID,
    p.Nome as ProdutoNome,
    pga.GrupoAdicionalID,
    ga.Nome as GrupoNome,
    pga.Ativo as AssociacaoAtiva
FROM Produto p
LEFT JOIN ProdutoGrupoAdicional pga ON p.ID = pga.ProdutoID AND pga.Ativo = 1
LEFT JOIN GrupoAdicional ga ON pga.GrupoAdicionalID = ga.ID
WHERE p.ID = 3383;

-- Verificar todos os produtos que têm grupos adicionais associados
SELECT 
    p.ID as ProdutoID,
    p.Nome as ProdutoNome,
    COUNT(pga.GrupoAdicionalID) as TotalGruposAssociados
FROM Produto p
LEFT JOIN ProdutoGrupoAdicional pga ON p.ID = pga.ProdutoID AND pga.Ativo = 1
WHERE p.EmpresaID = 1301
GROUP BY p.ID, p.Nome
HAVING COUNT(pga.GrupoAdicionalID) > 0;
