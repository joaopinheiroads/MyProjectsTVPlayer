
ALTER PROCEDURE [dbo].[SPGetProgramacaoPluginFolder]
	@usuarioID INTEGER = 0
AS

BEGIN

	SET NOCOUNT ON;	

	DECLARE @empresaID INT
	DECLARE @grupoID INT
	DECLARE @isAdmin BIT

	SET @grupoID = (SELECT GrupoID FROM UsuarioGrupo WHERE UsuarioGrupo.UsuarioID = @usuarioID)
	SET @empresaID = ISNULL((SELECT EmpresaID FROM UsuarioEmpresa WHERE UsuarioEmpresa.UsuarioID = @usuarioID), 0)
	SET @isAdmin = dbo.UsuarioIsAdmin (@usuarioID)

SELECT  
	CONVERT(varchar(10), Video.ID) AS ID,
	'F_1678' AS ParentID,
	Video.Nome AS Nome,
	Video.Duracao AS Duracao,
	NULL AS Param1,
	NULL AS Tipo,
	'fas fa-rss-square' AS Icone
FROM
	Video 
	INNER JOIN FeedsTipo ON Video.ID = FeedsTipo.VideoID 
	INNER JOIN FeedsUsuario ON FeedsTipo.ID = FeedsUsuario.FeedsTipoID	
WHERE 
	Video.IsSistema = 1 
	AND	FeedsUsuario.Ativo = 1 
	AND	FeedsTipo.Ativo = 1 
	AND	(Video.ID = 28984 OR Video.ID = 29718 OR Video.ID = 30571 OR Video.ID = 30103 OR Video.ID = 30282) 
	AND FeedsUsuario.UsuarioFeedsID = @usuarioID
		
UNION ALL
	
SELECT DISTINCT
	'6_' + CONVERT(VARCHAR(10), WebSite.ID) AS ID,
	'F_1678' AS ParentID,
	'Fanpage - ' + WebSite.Nome AS Nome,
	30 AS Duracao,
	WebSite.TipoID AS Param1,
	6 AS Tipo,
	'fab fa-facebook' AS Icone
FROM
	WebSite
	INNER JOIN UsuarioGrupo ON (((@isAdmin = 1) AND (WebSite.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID))
								  OR ((@isAdmin = 0) AND (WebSite.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID AND UsuarioGrupo.IsAdmin = 1)))	
WHERE
	UsuarioGrupo.GrupoID = @grupoID
	AND WebSite.Ativo = 1
	AND WebSite.TipoID = 2
	
UNION

SELECT DISTINCT
	'6_' + CONVERT(VARCHAR(10), WebSite.ID) AS ID,
	'F_1678' AS ParentID,
	'Fanpage - ' + WebSite.Nome AS Nome,
	30 AS Duracao,
	WebSite.TipoID AS Param1,
	6 AS Tipo,
	'fab fa-facebook' AS Icone
FROM
	WebSite
	INNER JOIN UsuarioEmpresa ON WebSite.UsuarioIDCadastrou = UsuarioEmpresa.UsuarioID AND UsuarioEmpresa.EmpresaID = @empresaID
WHERE
	WebSite.Ativo = 1
	AND WebSite.TipoID = 2			

UNION ALL

SELECT DISTINCT
	'7_' + CONVERT(VARCHAR(10), PluginFood.ID) AS ID,
	'F_1678' AS ParentID,
	'Food - ' + PluginFood.Nome AS Nome,
	11 AS Duracao,
	PluginFood.ID AS Param1,
	7 AS Tipo,
	'fas fa-utensils' AS Icone
FROM 
	PluginFood 
	INNER JOIN	UsuarioGrupo ON (((@isAdmin = 1) AND (PluginFood.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID))
								OR ((@isAdmin = 0) AND (PluginFood.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID AND UsuarioGrupo.IsAdmin = 1)))
WHERE 
	UsuarioGrupo.GrupoID = @grupoID 
	AND PluginFood.Ativo = 1	

UNION

SELECT DISTINCT
	'7_' + CONVERT(VARCHAR(10), PluginFood.ID) AS ID,
	'F_1678' AS ParentID,
	'Food - ' + PluginFood.Nome AS Nome,
	11 AS Duracao,
	PluginFood.ID AS Param1,
	7 AS Tipo,
	'fas fa-utensils' AS Icone
FROM 
	PluginFood 
	INNER JOIN UsuarioEmpresa ON PluginFood.UsuarioIDCadastrou = UsuarioEmpresa.UsuarioID AND UsuarioEmpresa.EmpresaID = @empresaID
WHERE 
	PluginFood.Ativo = 1

UNION ALL

SELECT DISTINCT
	'8_' + CONVERT(VARCHAR(10), PluginNoticiaCustom.ID) AS ID,
	'F_1678' AS ParentID,
	'Notícia - ' + PluginNoticiaCustom.Titulo AS Nome,
	ISNULL(LayoutPluginNoticia.Tempo, 15) AS Duracao,
	PluginNoticiaCustom.ID AS Param1,
	8 AS Tipo,
	'far fa-newspaper' AS Icone
FROM
	PluginNoticiaCustom 
	LEFT OUTER JOIN	LayoutPluginNoticia ON PluginNoticiaCustom.LayoutPluginNoticiaID = LayoutPluginNoticia.ID 
	INNER JOIN	UsuarioGrupo ON (((@isAdmin = 1) AND (PluginNoticiaCustom.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID))
								OR((@isAdmin = 0) AND (PluginNoticiaCustom.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID AND UsuarioGrupo.IsAdmin = 1)))
WHERE 
	UsuarioGrupo.GrupoID = @grupoID 
	AND PluginNoticiaCustom.Ativo = 1
	AND ISNULL(LayoutPluginNoticia.Ativo, 1) = 1

UNION

SELECT DISTINCT
	'8_' + CONVERT(VARCHAR(10), PluginNoticiaCustom.ID) AS ID,
	'F_1678' AS ParentID,
	'Notícia - ' + PluginNoticiaCustom.Titulo AS Nome,
	ISNULL(LayoutPluginNoticia.Tempo, 15) AS Duracao,
	PluginNoticiaCustom.ID AS Param1,
	8 AS Tipo,
	'far fa-newspaper' AS Icone
FROM
	PluginNoticiaCustom 
	LEFT OUTER JOIN	LayoutPluginNoticia ON PluginNoticiaCustom.LayoutPluginNoticiaID = LayoutPluginNoticia.ID 
	INNER JOIN UsuarioEmpresa ON PluginNoticiaCustom.UsuarioIDCadastrou = UsuarioEmpresa.UsuarioID AND UsuarioEmpresa.EmpresaID = @empresaID
WHERE 
	PluginNoticiaCustom.Ativo = 1
	AND ISNULL(LayoutPluginNoticia.Ativo, 1) = 1

UNION ALL

SELECT DISTINCT
	'9_' + CONVERT(VARCHAR(10), PluginContadorRegressivo.ID) AS ID,
	'F_1678' AS ParentID,
	'Contador - ' + PluginContadorRegressivo.NomeEvento AS Nome,
	15 AS Duracao,
	PluginContadorRegressivo.ID AS Param1,
	9 AS Tipo,
	'fas fa-sort-numeric-down' AS Icone
FROM
	PluginContadorRegressivo 		
	INNER JOIN UsuarioGrupo ON (((@isAdmin = 1) AND (PluginContadorRegressivo.UsuarioIDCadastro = UsuarioGrupo.UsuarioID))
								OR((@isAdmin = 0) AND (PluginContadorRegressivo.UsuarioIDCadastro = UsuarioGrupo.UsuarioID AND UsuarioGrupo.IsAdmin = 1)))
WHERE 
	UsuarioGrupo.GrupoID = @grupoID 
	AND PluginContadorRegressivo.Ativo = 1

UNION

SELECT DISTINCT
	'9_' + CONVERT(VARCHAR(10), PluginContadorRegressivo.ID) AS ID,
	'F_1678' AS ParentID,
	'Contador - ' + PluginContadorRegressivo.NomeEvento AS Nome,
	15 AS Duracao,
	PluginContadorRegressivo.ID AS Param1,
	9 AS Tipo,
	'fas fa-sort-numeric-down' AS Icone
FROM
	PluginContadorRegressivo 		
	INNER JOIN UsuarioEmpresa ON PluginContadorRegressivo.UsuarioIDCadastro = UsuarioEmpresa.UsuarioID AND UsuarioEmpresa.EmpresaID = @empresaID
WHERE 
	PluginContadorRegressivo.Ativo = 1

UNION ALL

SELECT DISTINCT
	'10_' + CONVERT(VARCHAR(10), PluginFestividade.ID) AS ID,
	'F_1678' AS ParentID,
	'Festividade - ' + PluginFestividade.NomeEvento AS Nome,
	15 AS Duracao,
	PluginFestividade.ID AS Param1,
	10 AS Tipo,
	'fas fa-glass-cheers' AS Icone
FROM
	PluginFestividade 		
	INNER JOIN UsuarioGrupo ON (((@isAdmin = 1) AND (PluginFestividade.UsuarioIDCadastro = UsuarioGrupo.UsuarioID))
								OR ((@isAdmin = 0) AND (PluginFestividade.UsuarioIDCadastro = UsuarioGrupo.UsuarioID AND UsuarioGrupo.IsAdmin = 1)))
WHERE 
	UsuarioGrupo.GrupoID = @grupoID 
	AND PluginFestividade.Ativo = 1

UNION

SELECT DISTINCT
	'10_' + CONVERT(VARCHAR(10), PluginFestividade.ID) AS ID,
	'F_1678' AS ParentID,
	'Festividade - ' + PluginFestividade.NomeEvento AS Nome,
	15 AS Duracao,
	PluginFestividade.ID AS Param1,
	10 AS Tipo,
	'fas fa-glass-cheers' AS Icone
FROM
	PluginFestividade 		
	INNER JOIN UsuarioEmpresa ON PluginFestividade.UsuarioIDCadastro = UsuarioEmpresa.UsuarioID AND UsuarioEmpresa.EmpresaID = @empresaID
WHERE 
	PluginFestividade.Ativo = 1

UNION ALL

SELECT 
	'11_' + CONVERT(VARCHAR(10), LayoutPluginMensagem.ID) AS ID,
	'F_1678' AS ParentID,
	'Informativo - ' + LayoutPluginMensagem.Nome AS Nome,
	10 AS Duracao,
	LayoutPluginMensagem.ID AS Param1,
	11 AS Tipo,
	'fas fa-info-circle' AS Icone
FROM
	LayoutPluginMensagem 		
	LEFT OUTER JOIN FeedsTipoUsuario ON FeedsTipoUsuario.FeedsTipoID = 6
WHERE 
	FeedsTipoUsuario.UsuarioID = @usuarioID
	AND LayoutPluginMensagem.Ativo = 1

UNION ALL

SELECT 
	'12_' + CONVERT(VARCHAR(10), FlashPlugin.ID) AS ID
	,'F_1678' AS ParentID
	, TipoFlashPlugin.Nome + ' - ' +  FlashPlugin.Nome AS Filename
	, 15 AS Duracao
	, FlashPlugin.ID AS Param1,
	12 AS Tipo,
	'fas fa-puzzle-piece' AS Icone
FROM 
	FlashPlugin 
	INNER JOIN TipoFlashPlugin ON FlashPlugin.TipoFlashPluginID = TipoFlashPlugin.ID AND FlashPlugin.Ativo = 1
	INNER JOIN UsuarioGrupo ON (((@isAdmin = 1) AND (FlashPlugin.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID))
								OR((@isAdmin = 0) AND (FlashPlugin.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID AND UsuarioGrupo.IsAdmin = 1)))
WHERE 
	FlashPlugin.Ativo = 1
	AND (FlashPlugin.IsSistema IS NULL OR FlashPlugin.IsSistema = 0)
	AND UsuarioGrupo.GrupoID = @grupoID	

UNION

SELECT 
	'12_' + CONVERT(VARCHAR(10), FlashPlugin.ID) AS ID
	,'F_1678' AS ParentID
	, TipoFlashPlugin.Nome + ' - ' +  FlashPlugin.Nome AS Filename
	, 15 AS Duracao
	, FlashPlugin.ID AS Param1,
	12 AS Tipo,
	'fas fa-puzzle-piece' AS Icone
FROM 
	FlashPlugin 
	INNER JOIN TipoFlashPlugin ON FlashPlugin.TipoFlashPluginID = TipoFlashPlugin.ID AND FlashPlugin.Ativo = 1
	INNER JOIN UsuarioEmpresa ON FlashPlugin.UsuarioIDCadastrou = UsuarioEmpresa.UsuarioID AND UsuarioEmpresa.EmpresaID = @empresaID
WHERE 
	FlashPlugin.Ativo = 1
	AND (FlashPlugin.IsSistema IS NULL OR FlashPlugin.IsSistema = 0)

UNION

SELECT 
	'12_' + CONVERT(VARCHAR(10), FlashPlugin.ID) AS ID
	,'F_1678' AS ParentID
	, TipoFlashPlugin.Nome + ' - ' +  FlashPlugin.Nome AS Filename
	, 15 AS Duracao
	, FlashPlugin.ID AS Param1,
	12 AS Tipo,
	'fas fa-puzzle-piece' AS Icone
FROM 
	FlashPlugin 
	INNER JOIN TipoFlashPlugin ON FlashPlugin.TipoFlashPluginID = TipoFlashPlugin.ID AND FlashPlugin.Ativo = 1
WHERE 
	FlashPlugin.Ativo = 1
	AND FlashPlugin.IsSistema = 1

UNION

SELECT DISTINCT
	'14_' + CONVERT(VARCHAR(10), HTML5Plugin.ID) AS ID,
	'F_1699' AS ParentID,
	'Plugin - ' + HTML5Plugin.Nome AS Filename,
	HTML5Plugin.Duracao AS Duracao,
	HTML5Plugin.ID AS Param1,
	14 AS Tipo,
	'fas fa-puzzle-piece' AS Icone
FROM
	HTML5Plugin
	CROSS APPLY (SELECT COUNT(*) AS vCount
				FROM HTML5PluginVariaveisFeed
				WHERE HTML5PluginID = HTML5Plugin.ID) VarCount
WHERE
	HTML5Plugin.Ativo = 1
	AND HTML5Plugin.UsuarioIDCadastrou = @usuarioID
	AND VarCount.vCount = 0

UNION

SELECT DISTINCT
	'14_' + CONVERT(VARCHAR(10), HTML5Plugin.ID) AS ID,
	'F_1699' AS ParentID,
	'Plugin - ' + HTML5Plugin.Nome AS Filename,
	HTML5Plugin.Duracao AS Duracao,
	HTML5Plugin.ID AS Param1,
	14 AS Tipo,
	'fas fa-puzzle-piece' AS Icone
FROM
	HTML5Plugin
	CROSS APPLY (SELECT COUNT(*) AS vCount
				FROM HTML5PluginVariaveisFeed
				WHERE HTML5PluginID = HTML5Plugin.ID) VarCount
	INNER JOIN UsuarioGrupo ON
	(((@isAdmin = 1) AND (HTML5Plugin.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID))
	OR ((@isAdmin = 0) AND (HTML5Plugin.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID AND UsuarioGrupo.IsAdmin = 1)))	
WHERE
	HTML5Plugin.Ativo = 1
	AND UsuarioGrupo.GrupoID = @grupoID	
	AND VarCount.vCount = 0

UNION

SELECT
	'16_' + CONVERT(VARCHAR(10), HTML5PluginTemplate.ID) AS ID,
	'F_1699' AS ParentID,
	'Feed Plugin - ' + HTML5PluginTemplate.Nome + ' (' + HTML5Plugin.Nome + ')' AS Filename,
	HTML5Plugin.Duracao AS Duracao,
	HTML5PluginTemplate.ID AS Param1,
	16 AS Tipo,
	'fas fa-puzzle-piece' AS Icone
FROM
	HTML5PluginTemplate
	INNER JOIN HTML5Plugin
		ON HTML5PluginTemplate.HTML5PluginID = HTML5Plugin.ID
WHERE
	HTML5PluginTemplate.Ativo = 1
	AND HTML5Plugin.Ativo = 1
	AND HTML5PluginTemplate.UsuarioIDCadastrou = @usuarioID

UNION

SELECT
	'16_' + CONVERT(VARCHAR(10), HTML5PluginTemplate.ID) AS ID,
	'F_1699' AS ParentID,
	'Feed Plugin - ' + HTML5PluginTemplate.Nome + ' (' + HTML5Plugin.Nome + ')' AS Filename,
	HTML5Plugin.Duracao AS Duracao,
	HTML5PluginTemplate.ID AS Param1,
	16 AS Tipo,
	'fas fa-puzzle-piece' AS Icone
FROM
	HTML5PluginTemplate
	INNER JOIN HTML5Plugin
		ON HTML5PluginTemplate.HTML5PluginID = HTML5Plugin.ID
	INNER JOIN	UsuarioGrupo
		ON (((@isAdmin = 1)
				AND (HTML5PluginTemplate.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID))
			OR((@isAdmin = 0)
				AND (HTML5PluginTemplate.UsuarioIDCadastrou = UsuarioGrupo.UsuarioID
				AND UsuarioGrupo.IsAdmin = 1)))
WHERE
	HTML5PluginTemplate.Ativo = 1
	AND HTML5Plugin.Ativo = 1
	AND UsuarioGrupo.GrupoID = @grupoID

UNION ALL

SELECT
	'18_' + CONVERT(VARCHAR(10), PluginsTVPlayer.ID) AS ID,
	'F_1678' AS ParentID,
	Nome,
	15 AS Duracao,
	PluginsTVPlayer.ID AS Param1,
	18 AS Tipo,
	Icone
FROM
	PluginsTVPlayer
WHERE
	Ativo = 1

UNION ALL

SELECT
	'18_14_' + CONVERT(VARCHAR(10), CategoriaRSSCanais.ID) AS ID,
	'18_14' AS ParentID,
	CategoriaRSSCanais.Nome,
	15 AS Duracao,
	PluginsTVPlayer.ID AS Param1,
	18 AS Tipo,
	PluginsTVPlayer.Icone
FROM
	CategoriaRSSCanais
	INNER JOIN PluginsTVPlayer ON PluginsTVPlayer.ID = 14 AND PluginsTVPlayer.Ativo = 1
WHERE
	CategoriaRSSCanais.CategoriaRSSID = 1
	AND CategoriaRSSCanais.Ativo = 1

UNION ALL


SELECT 
	CONVERT(VARCHAR(10),TipoMidia.ID) + '_' + CONVERT(VARCHAR(10), SocialPlugin.ID) AS ID,
	'F_1698' AS ParentID,
	CASE WHEN TipoMidia.ID = 24
	THEN 'Vídeo - ' + SocialPlugin.Nome
	ELSE SocialPlugin.Nome
	END AS Nome,
	15 AS Duracao,
	SocialPlugin.ID AS Param1,
	TipoMidia.ID AS Tipo,
	CASE WHEN TipoMidia.ID = 17
	THEN 'fab fa-facebook'
	ELSE 'fab fa-instagram'
	END AS Icone
FROM
	SocialPlugin
	JOIN TipoMidia ON SocialPlugin.TipoMidia = TipoMidia.ID
	LEFT OUTER JOIN UsuarioGrupo ON (((@isAdmin = 1) AND (SocialPlugin.UsuarioIDCadastro = UsuarioGrupo.UsuarioID AND UsuarioGrupo.GrupoID = @grupoID))
			OR ((@isAdmin = 0) AND (SocialPlugin.UsuarioIDCadastro = UsuarioGrupo.UsuarioID AND UsuarioGrupo.IsAdmin = 1 AND UsuarioGrupo.GrupoID = @grupoID)))
	LEFT OUTER JOIN UsuarioEmpresa ON SocialPlugin.UsuarioIDCadastro = UsuarioEmpresa.UsuarioID AND UsuarioEmpresa.EmpresaID = @empresaID	
WHERE
	(UsuarioGrupo.ID IS NOT NULL OR UsuarioEmpresa.ID IS NOT NULL)
	AND Ativo = 1

UNION ALL

	SELECT  
		'21_' + CONVERT(varchar(10), ID) AS ID,
		'F_1678' AS ParentID,
		'Plugin Transparente - Player Windows' AS Nome,
		15 AS Duracao,
		ID AS Param1,
		21 AS Tipo,
		'fas fa-puzzle-piece' AS Icone
	FROM
		Video
	WHERE 
		ID = 211254

--		UNION ALL

--SELECT  
--	'22_' + CONVERT(varchar(10), PluginCombustivel.ID) AS ID,
--	'F_1678' AS ParentID,
--	'Combustível - ' + PluginCombustivel.Nome AS Nome,
--	15 AS Duracao,
--	PluginCombustivel.ID AS Param1,
--	22 AS Tipo,
--	'fas fa-gas-pump' AS Icone
--FROM
--	PluginCombustivel 
--	INNER JOIN PluginTemplate ON PluginCombustivel.PluginTemplateID = PluginTemplate.ID
--	INNER JOIN	UsuarioGrupo
--	ON (((@isAdmin = 1)
--			AND (PluginCombustivel.UsuarioIDCadastro = UsuarioGrupo.UsuarioID))
--		OR((@isAdmin = 0)
--			AND (PluginCombustivel.UsuarioIDCadastro = UsuarioGrupo.UsuarioID
--			AND UsuarioGrupo.IsAdmin = 1)))
--WHERE 
--	UsuarioGrupo.GrupoID = @grupoID 
--	AND PluginCombustivel.Ativo = 1
--	AND PluginTemplate.Ativo = 1

ORDER BY
	ID, Nome


END
