var keyGlobal;
var targetKeyGlobal;
var isPlugin;
var scrollPosition;
var isYoutube = false;
var isPluginFootball = false;
var isAdmooh = false;
var isCanaltech = false;
var categoriaCanaltech = null;
var isJovemPan = false;
var categoriaJovemPan = null;
var plugins = ['6', '12', '17', '18', '19'];
var pluginID;
var naoDefinemTempo = ['2', '5', '7', '8', '9', '10', '14', '16', '24'];
var isInfoCampanha = false;
var infoCampanhaCampanhaID = 0;
var infoCampanhaProgID = 0;
var infoCampanhaTerminalID = 0;
var infoCampanhaDataInicio = '';
var infoCampanhaDataFim = '';
var infoCampanhaNome = '';
var addMode = 'drag'; // 'drag', 'end', 'position'
var sourceTreelistName = 'treVideoImagem';

function SetIsInfoCampanha(isInfo, campanhaID, progID, terminalID, dataInicio, dataFim, nome) {
    isInfoCampanha = isInfo;
    infoCampanhaCampanhaID = campanhaID;
    infoCampanhaProgID = progID;
    infoCampanhaTerminalID = terminalID;
    infoCampanhaDataInicio = dataInicio;
    infoCampanhaDataFim = dataFim;
    infoCampanhaNome = nome;
}

function ParseKeyAndSetGlobals(key) {
    isYoutube = false;
    isPluginFootball = false;
    isAdmooh = false;
    isCanaltech = false;
    categoriaCanaltech = null;
    isJovemPan = false;
    categoriaJovemPan = null;
    if (key.indexOf('_') == -1) {
        isPlugin = false;
    }
    else {
        isPlugin = true;
        pluginID = key.split('_')[0];

        if (pluginID == 18) {
            isYoutube = (key.split('_')[1] == '8');//youtube
            isPluginFootball = (key.split('_')[1] == '11');//football
            isAdmooh = (key.split('_')[1] == '10');
            isCanaltech = (key.split('_')[1] == '14');
            categoriaCanaltech = isCanaltech ? (key.split('_')[2] || null) : null;
            isJovemPan = (key.split('_')[1] == '18');
            categoriaJovemPan = isJovemPan ? (key.split('_')[2] || null) : null;
        }
    }
}

function ShouldUseProcessAddVideo(key) {
    ParseKeyAndSetGlobals(key);
    return isPlugin;
}

function ProcessAddVideo(key, targetKey, mode) {
    ParseKeyAndSetGlobals(key);
    addMode = mode || 'drag';
    
    keyGlobal = key;
    targetKeyGlobal = targetKey;

    if (categoriaCanaltech) {
        keyGlobal = key.split('_').slice(0, 2).join('_');
        if (SelecionarCategoriaCanaltech(categoriaCanaltech))
            pucTempoPlugin.Show();
        else
            pucCanaltech.Show();
    }
    else if (categoriaJovemPan) {
        keyGlobal = key.split('_').slice(0, 2).join('_');
        if (SelecionarCategoriaJovemPan(categoriaJovemPan))
            pucTempoPlugin.Show();
        else
            pucJovemPan.Show();
    }
    else if (isPlugin) {
        if (pluginID == '4')
            pucTipoCampanha.Show();
        else if (naoDefinemTempo.indexOf(pluginID) == '-1' && !isYoutube && !isAdmooh && !isPluginFootball && !isCanaltech && !isJovemPan)
            pucTempoPlugin.Show();
        else if (isYoutube)
            pucPluginYoutube.Show();
        else if (isPluginFootball)
            pucPluginFootball.Show();
        else if (isCanaltech)
            pucCanaltech.Show();
        else if (isJovemPan)
            pucJovemPan.Show();
        //else if (isCombustivel)
        //    pucCombustivel.Show();
        else
            ExecuteAddCallback();
    }
    else
        ExecuteAddCallback();
}

function SelecionarCategoriaCanaltech(categoria) {
    cbxCategoriaCanaltech.SetValue(parseInt(categoria, 10));
    return cbxCategoriaCanaltech.GetValue() != null;
}

function SelecionarCategoriaJovemPan(categoria) {
    cbxCategoriaJovemPan.SetValue(parseInt(categoria, 10));
    return cbxCategoriaJovemPan.GetValue() != null;
}

function ProcessAddVarios(keys, targetKey, mode) {
    if (keys.some(ChaveAbrePopupProprio)) {
        parent.bootbox.alert('Canaltech, YouTube, Futebol, Jovem Pan e Campanha precisam ser arrastados um por vez.');
        return;
    }
    addMode = mode || 'drag';
    keyGlobal = keys.join(',');
    targetKeyGlobal = targetKey;

    if (keys.some(ChaveDefineTempo))
        pucTempoPlugin.Show();
    else
        ExecuteAddCallback();
}

function ChaveAbrePopupProprio(key) {
    ParseKeyAndSetGlobals(key);
    return (isPlugin && pluginID == '4') || isYoutube || isPluginFootball || isCanaltech || isJovemPan;
}

function ChaveDefineTempo(key) {
    ParseKeyAndSetGlobals(key);
    return isPlugin && naoDefinemTempo.indexOf(pluginID) == -1 && !isAdmooh;
}

function ChavesDoArrasto(tree, chaveArrastada) {
    if (!tree.IsNodeSelected(chaveArrastada))
        return [chaveArrastada];
    return tree.GetVisibleSelectedNodeKeys().filter(function (chave) {
        return chave.indexOf('F') == -1;
    });
}

function ExecuteAddCallback() {
    var treeName = 'treVideoImagem';
    if (addMode === 'drag') {
        treeName = sourceTreelistName;
    } else {
        if (typeof treelistNome !== 'undefined' && treelistNome) treeName = treelistNome;
    }

    if (addMode === 'end') {
        calPanel.PerformCallback("addToTheEnd|" + keyGlobal + "|" + treeName);
    } else if (addMode === 'position') {
        // Para 'position', targetKeyGlobal contém a posição numérica
        calPanel.PerformCallback("addToPosition|" + targetKeyGlobal + "|" + keyGlobal + "|" + treeName);
    } else {
        // 'drag' mode
        calPanel.PerformCallback("addToPosition|" + targetKeyGlobal + "|" + keyGlobal + "|" + treeName);
    }
}

function OnControlsInitializedGloEventsProgramacao() {
    SetHeightDivVideoSelecionados();
}

function OnControlsInitializedGloEventsCampanha() {
    SetHeightDivVideoSelecionados();
}

function OnClickBtnInfoCampanha(campanhaID, progID, terminalID, dataInicio, dataFim, nome) {

    SetIsInfoCampanha(true, campanhaID, progID, terminalID, dataInicio, dataFim, nome);
    
    pucInfoCampanha.SetHeaderText("Arquivos da Campanha: " + nome);

    pucInfoCampanha.SetContentUrl('ProgramacaoCampanha/FrmListaVideosCampanha.aspx?campanhaID=' + campanhaID + '&progID=' + progID + '&terminalID=' + terminalID + '&dataInicio=' + dataInicio + '&dataFim=' + dataFim);
    pucInfoCampanha.Show();
    pucInfoCampanha.SetSize(530, 410);
    pucInfoCampanha.UpdatePosition();
}

function SetHeightTreVideoImagem() {
    treVideoImagem.SetHeight($('#divConteudoDisponivel').height() - 100);
}

function SetHeightDivVideoSelecionados() {
    treVideo.SetHeight($('.divConteudos').height() - 105);
}

function OnClickBtnDeleteVideoSelecionado(value) {
    calPanel.PerformCallback('delete' + ':' + value);
}

function OnClickRemoveAllConteudoProgramacao(s, e) {
    parent.bootbox.confirm({
        message: "Todo o conteúdo será removido. Deseja continuar?",
        callback: function (result) {
            if (result) calPanel.PerformCallback('removeall');
        }
    });
}

function OnClickRemoveSelected(s, e) {
    treVideo.GetSelectedNodeValues("Id", RemoveSelected);
}

function RemoveSelected(values) {
    if (values.length > 0)
        calPanel.PerformCallback('remove_selected;' + values.toString());
}

function OnClickRemoveAllConteudoCampanha(s, e) {
    parent.bootbox.confirm({
        message: "O conteúdo da campanha será removido. Deseja continuar?",
        callback: function (result) {
            if (result) treVideo.PerformCallback('removeall');
        }
    });
}

function OnClickSalvar(s, e) {
    var nodes = treVideo.GetVisibleNodeKeys();
    if (nodes == '') {
        parent.bootbox.alert("Selecione pelo menos um vídeo/imagem ou rss!");
        return;
    }
    if (ASPxClientEdit.ValidateGroup('ValidateProgName')) {
        btnSalvar.SetEnabled(false);
        calCallback.PerformCallback('salvar');
    }
}

function SetHdfScrollPositionVideoSelecionado() {
    var scrollH = document.getElementById('divVideoSelecionados').scrollHeight;
    var divH = parseFloat(document.getElementById('divVideoSelecionados').style.height.replace('px', ''));
    var result = scrollH - divH;
    if (result == document.getElementById("divVideoSelecionados").scrollTop)
        document.getElementById("calPanel_hdfScrollPositionVideoSelecionado").value = 'max';
    else
        document.getElementById("calPanel_hdfScrollPositionVideoSelecionado").value = document.getElementById('divVideoSelecionados').scrollTop;
}

function OnEndCallbackCalPanel(s, e) {
    treVideoImagem.PerformCallback();
    if (document.getElementById("calPanel_hdfScrollPositionVideoSelecionado").value == 'max') {
        var scrollH = document.getElementById('divVideoSelecionados').scrollHeight;
        var divH = parseFloat(document.getElementById('divVideoSelecionados').style.height.replace('px', ''));
        var result = scrollH - divH;
        document.getElementById('divVideoSelecionados').scrollTop = result;
    }
    else
        document.getElementById('divVideoSelecionados').scrollTop = document.getElementById("calPanel_hdfScrollPositionVideoSelecionado").value;
    ResizeDivs();
}

function SetHdfDuracaoMidia(valor) {
    document.getElementById('calPanel_hdfDuracaoMidia').value = valor;
}

function OnClickBtnOKPopup(popup) {
    if (popup.name.indexOf('pucPluginYoutube') != -1) {
        var tempo = tmeTempoYoutube.GetText();
        var totalSec = 0;
        var spl = [];
        spl = tempo.split(':');
        totalSec = spl[0] != '00' ? parseInt(spl[0]) * 3600 : 0;
        totalSec += spl[1] != '00' ? parseInt(spl[1]) * 60 : 0;
        totalSec += spl[2] != '00' ? parseInt(spl[2]) : 0;
        SetHdfDuracaoMidia(totalSec);
    }
    else if (popup.name.indexOf('pucPluginFootball') != -1) {
        SetHdfDuracaoMidia(speTempoFutebol.GetValue());
    }
    else if (popup.name.indexOf('pucCanaltech') != -1) {
        SetHdfDuracaoMidia(speTempoCanaltech.GetValue());
    }
    else if (popup.name.indexOf('pucJovemPan') != -1) {
        SetHdfDuracaoMidia(speTempoJovemPan.GetValue());
    }
    else if (popup.name.indexOf('pucCombustivel') != -1) {
        if (!ASPxClientEdit.ValidateGroup('groupCombustivel'))
            return;
        SetHdfDuracaoMidia(speTempoCombustivel.GetValue());
    }
    popup.Hide();

    ExecuteAddCallback();
}

function OnClickCancelarPopup(popup) {
    popup.Hide();
}

var posX, posY;
function OnSelecionadosContext(s, e) {
    console.log('e.objectKey: ' + e.objectKey);
    console.log('e.objectType: ' + e.objectType);
    posX = ASPxClientUtils.GetEventX(e.htmlEvent);
    posY = ASPxClientUtils.GetEventY(e.htmlEvent)
    //treVideo.GetSelectedNodeValues("StrVideoId", GetNodeValueSelecionadoForContext, false);
}

function ItemClickSelecionados(s, e) {
    const selected = e.item.name;
    switch (selected) {
        case 'iVisualizar':
            treVideo.GetSelectedNodeValues("StrVideoId", GetNodeValueSelecionadoForPreview, false);
            break;
        default: break;
    }
}

function ChangePositionOfSelecteds() {
    treVideo.GetSelectedNodeValues("Id", GetNodeValueSelecionadoForChangePosition, false);
}

function GetNodeValueSelecionadoForChangePosition(values) {
    if (values.length <= 0)
        return;
    const position = document.querySelector('.inputSelectPosition').value;
    calPanel.PerformCallback("changePosition|" + values + "|" + (position > 0 ? position : 1));
}

function GetNodeValueSelecionadoForContext(values) {
    let showVisualizar = false;
    let position = values[0].indexOf('_');
    if (position != -1)
        showVisualizar = (values[0].substr(0, position) == '1');
    else
        showVisualizar = true;

    ppMenuSelecionados.GetItemByName('iVisualizar').SetVisible(showVisualizar);

    ppMenuSelecionados.ShowAtPos(posX, posY);
}

function GetNodeValueSelecionadoForPreview(values) {

    SetIsInfoCampanha(false, 0, 0, 0, '', '', '');

    let nodeKey = values[0];
    if (nodeKey.indexOf('_') != -1)
        nodeKey = nodeKey.split('_')[1];
    pucPopup.SetHeaderText('Preview');
    pucPopup.SetContentUrl('../Pages/FrmPreviewVideo.aspx?showControls=1&video=' + nodeKey);
    pucPopup.SetSize(600, 500);
    pucPopup.Show();
    pucPopup.UpdatePosition();
}

function GetNodeValueSelecionadoForPreviewTerminal(values, terminalID)
{
    SetIsInfoCampanha(false, 0, 0, 0, '', '', '');

    let nodeKey = values[0];
    if (nodeKey.indexOf('_') != -1)
        nodeKey = nodeKey.split('_')[1];
    pucPopup.SetHeaderText('Preview');
    pucPopup.SetContentUrl('../Pages/FrmPreviewVideo.aspx?showControls=1&video=' + nodeKey + '&terminalID=' + terminalID);
    pucPopup.SetSize(600, 500);
    pucPopup.Show();
    pucPopup.UpdatePosition();
}

function OpenPreview(videoID, terminalID) {
    pucPopup.SetHeaderText('Preview');
    pucPopup.SetContentUrl('../Pages/FrmPreviewVideo.aspx?showControls=1&video=' + videoID + '&terminalID=' + terminalID);
    pucPopup.SetSize(600, 500);
    pucPopup.Show();
    pucPopup.UpdatePosition();
}

function OnValueChangedRblTipoYoutube() {
    if (rblTipoYoutube.GetValue() == false) {
        txtYoutubeVideo.SetCaption('ID do Vídeo');
    }
    else {
        txtYoutubeVideo.SetCaption('ID da Playlist');
    }
}

function ShowVideoImageInfo(StrVideoId) {
    let nodeKey = StrVideoId[0];
    if (nodeKey.indexOf('_') != -1)
        nodeKey = nodeKey.split('_')[1];
    
    pucPopup.SetHeaderText('Preview');
    pucPopup.SetContentUrl('../Pages/FrmPreviewVideo.aspx?showControls=1&video=' + nodeKey);
    pucPopup.SetSize(600, 500);
    pucPopup.Show();
    pucPopup.UpdatePosition();
}

function SubstituirMidia(midiaID, midiaNome) {
    pucSubstituirMidia.SetHeaderText('Substituição de Mídia');
    pucSubstituirMidia.SetContentUrl('Ferramentas/FrmSubstituicaoVideoDetalhe.aspx?midiaID=' + midiaID + '&midiaNome=' + midiaNome);
    pucSubstituirMidia.SetSize(720, 500);
    pucSubstituirMidia.Show();
    pucSubstituirMidia.UpdatePosition();
}

function OnCloseUpPucSubstituirMidia(s, e) {
    pucSubstituirMidia.SetContentUrl('../Blank.aspx');
}

function ClosePopUpSubstituicaoVideo(oldVideoID, newVideoID)
{
    pucPopup.Hide();
    pucSubstituirMidia.Hide();
    if (isInfoCampanha) {
        OnClickBtnInfoCampanha(infoCampanhaCampanhaID, infoCampanhaProgID, infoCampanhaTerminalID, infoCampanhaDataInicio, infoCampanhaDataFim, infoCampanhaNome);
    }
    treVideo.PerformCallback('substituir' + ':' + oldVideoID + ':' + newVideoID);
}

function OnClickBtnEditarCampanha(campanhaID)
{
    document.location = "../FrmCampanhaDetalhe.aspx?campID=" + campanhaID;
}

function OnBtnAtualizarClick(s, e)
{
    calPanel.PerformCallback('refresh');
}





/*


function isMobile() {
    // Define o ponto de interrupção (breakpoint) para dispositivos móveis
    // (ex: 768px é um breakpoint comum, ajuste conforme necessário para seu design)
    const mobileBreakpoint = 768;
    return window.matchMedia(`(max-width: ${mobileBreakpoint}px)`).matches;
}

// Exemplo de uso:
if (isMobile()) {


    MyDragHelper = {
        StartDragNode: function (s, e) {
            e.cancel = true;  // Cancela o drag imediatamente, nem começa
            return;
        },
        EndDragNode: function (s, e) {
            // Nem precisa fazer nada aqui, mas por segurança
            e.cancel = true;
        },
        CreateTargets: function (targets) {
            // Nem vai ser chamado na maioria dos casos, mas se for...
            targets.splice(0, targets.length);
        },
        GetTargetTree: function (element) {
            return null;
        }
    }




    MyDragHelperReorder = {
        StartDragNode: function (s, e) {
            // mantém o ciclo vivo
            MyDragHelperReorder.CreateTargets(e.targets);
            e.cancel = true; // bloqueia drag
        },
        EndDragNode: function (s, e) {
            e.cancel = true;
        },
        CreateTargets: function (targets) {
            targets.splice(0, targets.length);

            var keys = treVideo.GetVisibleNodeKeys();
            for (var i = 0; i < keys.length; i++) {
                var el = treVideo.GetNodeHtmlElement(keys[i]);
                if (el) targets.push(el);
            }

            var root = document.getElementById("__rightRoot");
            if (root) targets.push(root);
            
        }
    };



    console.log("A tela está em modo mobile.");


} else {

    MyDragHelper = {
        StartDragNode: function (s, e) {
            if (e.nodeKey.indexOf('F') != -1) {
                e.cancel = true;
                return;
            }

            if (typeof treVideoImagem !== 'undefined' && s === treVideoImagem) sourceTreelistName = "treVideoImagem";
            else if (typeof treConteudoProprio !== 'undefined' && s === treConteudoProprio) sourceTreelistName = "treConteudoProprio";
            else if (typeof treRepositorio !== 'undefined' && s === treRepositorio) sourceTreelistName = "treRepositorio";
            else sourceTreelistName = "treVideoImagem";

            MyDragHelper.CreateTargets(e.targets);
            ParseKeyAndSetGlobals(e.nodeKey);
            document.getElementById('calPanel_hdfScrollPosition').value = document.getElementById("divVideoImagem").scrollTop;
        },
        EndDragNode: function (s, e) {
            SetHdfScrollPositionVideoSelecionado();
            var key = e.nodeKey;
            var targetKey;
            if (e.targetElement.id == "__rightRoot")
                targetKey = 0;
            else
                targetKey = treVideo.GetNodeKeyByRow(e.targetElement);

            ProcessAddVideo(key, targetKey, 'drag');
            //calPanel.PerformCallback("dragVideo|" + key + "|" + targetKey);

            e.cancel = true;
        },
        CreateTargets: function (targets) {
            targets.splice(0, targets.length);
            var list = [treVideo];
            for (var i = 0; i < list.length; i++) {
                var tree = list[i];
                var keys = tree.GetVisibleNodeKeys();
                for (var j = 0; j < keys.length; j++) {
                    if (IsNodeVisible(tree.GetNodeHtmlElement(keys[j])))
                        targets.push(tree.GetNodeHtmlElement(keys[j]));
                }
            }
            targets.push(document.getElementById("__rightRoot"));
        },
        GetTargetTree: function (element) {
            var rightElement = treVideo.GetMainElement();
            while (element) {
                if (element == rightElement)
                    return treVideo;
                element = element.parentNode;
            }
        }
    }





    MyDragHelperReorder = {
        StartDragNode: function (s, e) {
            window.isDragging = true;
            MyDragHelperReorder.CreateTargets(e.targets);
        },
        EndDragNode: function (s, e) {
            SetHdfScrollPositionVideoSelecionado();
            var key;
            if (e.targetElement.id == "__rightRoot")
                key = 0;
            else
                key = s.GetNodeKeyByRow(e.targetElement);
            s.PerformCallback('reorder' + ':' + e.nodeKey + ':' + key);
            e.cancel = true;
        },

        CreateTargets: function (targets) {
            targets.splice(0, targets.length);
            var list = [treVideo];
            for (var i = 0; i < list.length; i++) {
                var tree = list[i];
                var keys = tree.GetVisibleNodeKeys();
                for (var j = 0; j < keys.length; j++)
                    targets.push(tree.GetNodeHtmlElement(keys[j]));
            }
            targets.push(document.getElementById("__rightRoot"));
        },

        GetTargetTree: function (element) {
            var rightElement = treVideo.GetMainElement();
            while (element) {
                if (element == rightElement)
                    return treVideo;
                element = element.parentNode;
            }
        }
    }



    console.log("A tela não está em modo mobile.");




} 

*/






function verificarToque() {
    if ('ontouchstart' in window || navigator.maxTouchPoints > 0 || navigator.msMaxTouchPoints > 0) {


        MyDragHelper = {
            StartDragNode: function (s, e) {
                e.cancel = true;  // Cancela o drag imediatamente, nem começa
                return;
            },
            EndDragNode: function (s, e) {
                // Nem precisa fazer nada aqui, mas por segurança
                e.cancel = true;
            },
            CreateTargets: function (targets) {
                // Nem vai ser chamado na maioria dos casos, mas se for...
                targets.splice(0, targets.length);
            },
            GetTargetTree: function (element) {
                return null;
            }
        }




        MyDragHelperReorder = {
            StartDragNode: function (s, e) {
                // mantém o ciclo vivo
                MyDragHelperReorder.CreateTargets(e.targets);
                e.cancel = true; // bloqueia drag
            },
            EndDragNode: function (s, e) {
                e.cancel = true;
            },
            CreateTargets: function (targets) {
                targets.splice(0, targets.length);

                var keys = treVideo.GetVisibleNodeKeys();
                for (var i = 0; i < keys.length; i++) {
                    var el = treVideo.GetNodeHtmlElement(keys[i]);
                    if (el) targets.push(el);
                }

                var root = document.getElementById("__rightRoot");
                if (root) targets.push(root);

            }
        };

        console.log("Dispositivo com suporte a toque (Touch)");
        // Código para mobile/tablet
    } else {



        MyDragHelper = {
            StartDragNode: function (s, e) {
                if (e.nodeKey.indexOf('F') != -1) {
                    e.cancel = true;
                    return;
                }

                if (typeof treVideoImagem !== 'undefined' && s === treVideoImagem) sourceTreelistName = "treVideoImagem";
                else if (typeof treConteudoProprio !== 'undefined' && s === treConteudoProprio) sourceTreelistName = "treConteudoProprio";
                else if (typeof treRepositorio !== 'undefined' && s === treRepositorio) sourceTreelistName = "treRepositorio";
                else sourceTreelistName = "treVideoImagem";

                MyDragHelper.CreateTargets(e.targets);
                ParseKeyAndSetGlobals(e.nodeKey);
                document.getElementById('calPanel_hdfScrollPosition').value = document.getElementById("divVideoImagem").scrollTop;
            },
            EndDragNode: function (s, e) {
                SetHdfScrollPositionVideoSelecionado();
                var key = e.nodeKey;
                var keys = ChavesDoArrasto(s, key);
                var targetKey;
                if (e.targetElement.id == "__rightRoot")
                    targetKey = 0;
                else
                    targetKey = treVideo.GetNodeKeyByRow(e.targetElement);

                if (keys.length > 1)
                    ProcessAddVarios(keys, targetKey, 'drag');
                else
                    ProcessAddVideo(key, targetKey, 'drag');

                e.cancel = true;
            },
            CreateTargets: function (targets) {
                targets.splice(0, targets.length);
                var list = [treVideo];
                for (var i = 0; i < list.length; i++) {
                    var tree = list[i];
                    var keys = tree.GetVisibleNodeKeys();
                    for (var j = 0; j < keys.length; j++) {
                        if (IsNodeVisible(tree.GetNodeHtmlElement(keys[j])))
                            targets.push(tree.GetNodeHtmlElement(keys[j]));
                    }
                }
                targets.push(document.getElementById("__rightRoot"));
            },
            GetTargetTree: function (element) {
                var rightElement = treVideo.GetMainElement();
                while (element) {
                    if (element == rightElement)
                        return treVideo;
                    element = element.parentNode;
                }
            }
        }





        MyDragHelperReorder = {
            StartDragNode: function (s, e) {
                window.isDragging = true;
                MyDragHelperReorder.CreateTargets(e.targets);
            },
            EndDragNode: function (s, e) {
                SetHdfScrollPositionVideoSelecionado();
                var key;
                if (e.targetElement.id == "__rightRoot")
                    key = 0;
                else
                    key = s.GetNodeKeyByRow(e.targetElement);
                s.PerformCallback('reorder' + ':' + e.nodeKey + ':' + key);
                e.cancel = true;
            },

            CreateTargets: function (targets) {
                targets.splice(0, targets.length);
                var list = [treVideo];
                for (var i = 0; i < list.length; i++) {
                    var tree = list[i];
                    var keys = tree.GetVisibleNodeKeys();
                    for (var j = 0; j < keys.length; j++)
                        targets.push(tree.GetNodeHtmlElement(keys[j]));
                }
                targets.push(document.getElementById("__rightRoot"));
            },

            GetTargetTree: function (element) {
                var rightElement = treVideo.GetMainElement();
                while (element) {
                    if (element == rightElement)
                        return treVideo;
                    element = element.parentNode;
                }
            }
        }

        console.log("Dispositivo sem toque (Desktop)");
        // Código para desktop
    }
}

verificarToque();