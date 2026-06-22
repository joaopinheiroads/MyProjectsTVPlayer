// Função para aguardar renderização completa da imagem
window.waitForImageRender = function(imageSelector, callback) {
    const checkImage = () => {
        const imageElement = document.querySelector(imageSelector);
        if (imageElement) {
            // Se a imagem já está carregada
            if (imageElement.complete && imageElement.naturalHeight !== 0) {
                setTimeout(callback, 50); // Pequeno delay adicional para garantir CSS aplicado
                return;
            }
            
            // Se ainda não carregou, aguarda o evento load
            imageElement.onload = () => {
                setTimeout(callback, 50);
            };
            
            // Fallback caso a imagem não carregue
            setTimeout(callback, 500);
        } else {
            // Se o elemento não existe ainda, tenta novamente
            setTimeout(checkImage, 50);
        }
    };
    
    checkImage();
};

// Função para forçar re-renderização do CSS da imagem
window.forceImageRerender = function(selector) {
    const element = document.querySelector(selector);
    if (element) {
        // Força recalculo do CSS
        element.style.display = 'none';
        element.offsetHeight; // Trigger reflow
        element.style.display = '';
        
        // Aguarda um frame para garantir aplicação
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                console.log('Image CSS re-applied');
            });
        });
    }
};

// Função para aguardar elemento aparecer no DOM
window.waitForElement = function(selector, callback, timeout = 5000) {
    const startTime = Date.now();
    
    const checkElement = () => {
        const element = document.querySelector(selector);
        if (element) {
            callback(element);
            return;
        }
        
        if (Date.now() - startTime < timeout) {
            setTimeout(checkElement, 50);
        } else {
            console.warn(`Element ${selector} not found within timeout`);
        }
    };
    
    checkElement();
};
