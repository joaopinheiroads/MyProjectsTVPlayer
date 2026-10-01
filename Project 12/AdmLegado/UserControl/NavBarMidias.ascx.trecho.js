    function SelecaoTemItemComPopupProprio(values) {
        return typeof ChaveAbrePopupProprio === 'function' && values.some(ChaveAbrePopupProprio);
    }

    function AvisarAdicionarUmPorVez() {
        parent.bootbox.alert('Canaltech, YouTube, Futebol, Jovem Pan e Campanha precisam ser adicionados um por vez.');
    }

    function OnGetSelectedValuesToAddEnd(values) {
        if (values != null && values.length > 0) {
            // Prioriza campanha se houver
            var campaigns = values.filter(function (id) { return id.toString().indexOf('4_') === 0; });

            if (campaigns.length > 0) {
                if (campaigns.length > 1) {
                    parent.bootbox.alert("Selecione apenas uma campanha por vez.");
                    return;
                }
                ProcessAddVideo(campaigns[0], 0);
                return;
            }

            if (values.length > 1) {
                if (SelecaoTemItemComPopupProprio(values)) {
                    AvisarAdicionarUmPorVez();
                    return;
                }
                calPanel.PerformCallback("addToTheEnd|" + values + "|" + treelistNome);
                return;
            }

            // Se for apenas um item, verifica se precisa de configuração (popup)
            if (typeof ShouldUseProcessAddVideo === 'function' && ShouldUseProcessAddVideo(values[0])) {
                ProcessAddVideo(values[0], 0, 'end');
            } else {
                calPanel.PerformCallback("addToTheEnd|" + values + "|" + treelistNome);
            }
        }
        else {
            parent.bootbox.alert('Favor selecionar ao menos um item');
        }
    }

    function BtnOkPositionClick() {
        ppcSelectPosition.Hide();
        SetHdfDuracaoMidia(speTempoBatch.GetText());

        var campaigns = selecteds.filter(function (id) { return id.toString().indexOf('4_') === 0; });

        if (campaigns.length > 0) {
            if (campaigns.length > 1) {
                parent.bootbox.alert("Selecione apenas uma campanha por vez.");
                return;
            }
            ProcessAddVideo(campaigns[0], spePosicao.GetValue(), 'position');
            return;
        }

        if (selecteds.length > 1) {
            if (SelecaoTemItemComPopupProprio(selecteds)) {
                AvisarAdicionarUmPorVez();
                return;
            }
            calPanel.PerformCallback("addToPosition|" + spePosicao.GetValue() + "|" + selecteds + "|" + treelistNome);
            return;
        }

        if (typeof ShouldUseProcessAddVideo === 'function' && ShouldUseProcessAddVideo(selecteds[0])) {
            ProcessAddVideo(selecteds[0], spePosicao.GetValue(), 'position');
        } else {
            calPanel.PerformCallback("addToPosition|" + spePosicao.GetValue() + "|" + selecteds + "|" + treelistNome);
        }
    }
