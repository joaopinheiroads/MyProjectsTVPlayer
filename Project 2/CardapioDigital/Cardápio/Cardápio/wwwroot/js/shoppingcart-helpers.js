// Funções auxiliares para o Shopping Cart
window.ShoppingCartHelpers = {
    ensureCartPosition: function() {
        const cartElement = document.querySelector('.container-cart');
        if (cartElement) {
            
            // Força a aplicação das classes corretas
            if (cartElement.classList.contains('open')) {
                cartElement.style.right = '0px';
                cartElement.style.position = 'fixed';
                cartElement.style.zIndex = '99999';
            } else {
                cartElement.style.right = '-500px';
                cartElement.style.position = 'fixed';
                cartElement.style.zIndex = '99999';
            }
        } else {
            console.log('[Cart JS] ERRO: Elemento do carrinho não encontrado');
        }
    },

    openCart: function() {
        const cartElement = document.querySelector('.container-cart');
        if (cartElement) {
            // Força propriedades críticas
            cartElement.style.position = 'fixed';
            cartElement.style.zIndex = '99999';
            cartElement.style.top = '62px';
            cartElement.style.right = '0px';
            
            cartElement.classList.add('open');
            document.body.classList.add('no-scroll');
            return true;
        }
        return false;
    },

    closeCart: function() {
        const cartElement = document.querySelector('.container-cart');
        if (cartElement) {
            // Força propriedades críticas
            cartElement.style.position = 'fixed';
            cartElement.style.zIndex = '99999';
            cartElement.style.top = '62px';
            cartElement.style.right = '-500px';
            
            cartElement.classList.remove('open');
            document.body.classList.remove('no-scroll');
            return true;
        }
        return false;
    }
};

// Função para verificar se é mobile
window.isMobile = function() {
    return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
};

// Observer para mudanças no DOM
const observer = new MutationObserver(function(mutations) {
    mutations.forEach(function(mutation) {
        if (mutation.type === 'childList' || mutation.type === 'attributes') {
            // Verifica se o carrinho foi adicionado ao DOM
            const cartElement = document.querySelector('.container-cart');
            if (cartElement && !cartElement.hasAttribute('data-initialized')) {
                cartElement.setAttribute('data-initialized', 'true');
                window.ShoppingCartHelpers.ensureCartPosition();
            }
        }
    });
});

// Inicia o observer quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function() {
    observer.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['class']
    });
});
