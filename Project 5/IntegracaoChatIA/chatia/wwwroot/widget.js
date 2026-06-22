/* Widget do Agente de IA — botão flutuante (arrastável) + painel de chat.
   Recursos:
   - Conversa persistida em sessionStorage (sobrevive a F5 enquanto a aba estiver aberta).
   - Botão arrastável; posição salva em localStorage.
   - Injeta "Falar com IA" no menu Suporte (abre o mesmo painel).
   Injetar em Pages/Default.aspx antes de </body>:
     <script src="https://191.6.5.106:44909/chatia/widget.js" defer></script>
   Detecta sozinho a URL base a partir do próprio src (funciona em /chatia/). */
(function () {
  "use strict";

  var me = document.currentScript || (function () {
    var s = document.getElementsByTagName("script");
    return s[s.length - 1];
  })();
  var BASE = me.src.replace(/widget\.js(\?.*)?$/, "");
  var API = BASE + "api/chat";

  var PKEY = "tvp_chat_btnpos";    // localStorage: posição do botão
  var SKEY = "tvp_chat_session";   // sessionStorage: id anônimo da sessão (para métricas)

  var SESSION_ID = getSessionId();

  // Conversa fica só em MEMÓRIA: persiste enquanto o usuário navega no painel
  // (o widget vive no shell, que não recarrega ao trocar de tela). Ao sair/deslogar
  // ou recarregar a página, a conversa é descartada — não fica salva.
  var history = [];                // [{role, content}]
  var streaming = false;
  var greeted = false;

  // ---------- estilos ----------
  var css = "" +
    ".tvp-chat-btn{position:fixed;right:24px;bottom:24px;width:60px;height:60px;border-radius:50%;background:linear-gradient(to right,#f06,#5cc6d0);color:#fff;border:none;cursor:grab;box-shadow:0 4px 14px rgba(0,0,0,.3);z-index:2147483600;font-size:26px;display:flex;align-items:center;justify-content:center;touch-action:none;user-select:none}" +
    ".tvp-chat-btn:hover{filter:brightness(1.08)}" +
    ".tvp-chat-btn.tvp-dragging{cursor:grabbing}" +
    ".tvp-chat-panel{position:fixed;right:24px;bottom:96px;width:370px;max-width:calc(100vw - 32px);height:520px;max-height:calc(100vh - 130px);background:#fff;border-radius:12px;box-shadow:0 8px 30px rgba(0,0,0,.35);z-index:2147483601;display:none;flex-direction:column;overflow:hidden;font-family:'Roboto',Arial,sans-serif}" +
    ".tvp-chat-panel.open{display:flex}" +
    ".tvp-chat-head{background:linear-gradient(to right,#f06,#5cc6d0);color:#fff;padding:14px 16px;font-weight:bold;display:flex;justify-content:space-between;align-items:center}" +
    ".tvp-chat-head .tvp-x{cursor:pointer;font-size:20px;line-height:1;opacity:.85}" +
    ".tvp-chat-head .tvp-x:hover{opacity:1}" +
    ".tvp-head-actions{display:flex;gap:12px;align-items:center}" +
    ".tvp-admin{cursor:pointer;font-size:13px;opacity:.9;display:none}" +
    ".tvp-admin:hover{opacity:1;text-decoration:underline}" +
    ".tvp-chat-body{flex:1;overflow-y:auto;padding:14px;background:#f5f6f8}" +
    ".tvp-msg{margin:8px 0;display:flex}" +
    ".tvp-msg.user{justify-content:flex-end}" +
    ".tvp-bubble{max-width:82%;padding:9px 12px;border-radius:12px;font-size:14px;line-height:1.45;white-space:normal;word-wrap:break-word}" +
    ".tvp-msg.user .tvp-bubble{background:linear-gradient(to right,#f06,#5cc6d0);color:#fff;border-bottom-right-radius:3px}" +
    ".tvp-msg.bot .tvp-bubble{background:#fff;color:#222;border:1px solid #e3e5e8;border-bottom-left-radius:3px}" +
    ".tvp-bubble ul{margin:6px 0;padding-left:20px}" +
    ".tvp-bubble .tvp-h{font-weight:bold;margin:8px 0 4px}" +
    ".tvp-bubble code{background:#eef0f4;padding:1px 5px;border-radius:4px;font-size:13px}" +
    ".tvp-bubble hr{border:none;border-top:1px solid #e3e5e8;margin:8px 0}" +
    ".tvp-video-btn{display:inline-flex;align-items:center;gap:6px;margin-top:10px;background:linear-gradient(to right,#f06,#5cc6d0);color:#fff;border:none;border-radius:8px;padding:7px 14px;cursor:pointer;font-size:13px}" +
    ".tvp-video-btn:hover{filter:brightness(1.06)}" +
    ".tvp-foot{display:flex;border-top:1px solid #e3e5e8;padding:8px;gap:8px;background:#fff}" +
    ".tvp-foot textarea{flex:1;resize:none;border:1px solid #ccd0d6;border-radius:8px;padding:8px 10px;font-size:14px;font-family:inherit;height:42px;max-height:120px}" +
    ".tvp-foot button{background:linear-gradient(to right,#f06,#5cc6d0);color:#fff;border:none;border-radius:8px;padding:0 16px;cursor:pointer;font-size:14px}" +
    ".tvp-foot button:disabled{opacity:.5;cursor:default}" +
    ".tvp-typing{font-size:13px;color:#888;padding:2px 4px}" +
    "@media (max-width:480px){.tvp-chat-panel{border-radius:14px 14px 0 0}.tvp-chat-btn{width:54px;height:54px;font-size:24px}.tvp-foot textarea{font-size:16px}}";
  var st = document.createElement("style");
  st.textContent = css;
  document.head.appendChild(st);

  // ---------- elementos ----------
  var btn = el("button", "tvp-chat-btn", { title: "Agente de IA — arraste para mover" });
  btn.innerHTML = "&#128172;"; // 💬
  var panel = el("div", "tvp-chat-panel");
  panel.innerHTML =
    '<div class="tvp-chat-head"><span>Agente de IA - BETA</span><span class="tvp-head-actions"><a class="tvp-admin" title="Gerenciar FAQ">&#9881; FAQ</a><span class="tvp-x" title="Fechar">&times;</span></span></div>' +
    '<div class="tvp-chat-body"></div>' +
    '<div class="tvp-foot"><textarea placeholder="Tire sua dúvida sobre o painel..."></textarea><button>Enviar</button></div>';
  document.body.appendChild(btn);
  document.body.appendChild(panel);

  var body = panel.querySelector(".tvp-chat-body");
  var input = panel.querySelector("textarea");
  var sendBtn = panel.querySelector(".tvp-foot button");

  // Botão "Gerenciar FAQ" — só aparece quando o painel passou o token do admin master.
  var adminToken = (typeof window.tvpChatAdminToken === "string") ? window.tvpChatAdminToken.trim() : "";
  if (adminToken) {
    var adminBtn = panel.querySelector(".tvp-admin");
    adminBtn.style.display = "inline";
    adminBtn.addEventListener("click", function () {
      window.open(BASE + "admin.html#t=" + encodeURIComponent(adminToken), "_blank");
    });
  }

  // restaura posição salva do botão
  applySavedPos();

  // ---------- abrir/fechar ----------
  function openChat() {
    panel.classList.add("open");
    positionPanel();
    if (!greeted && history.length === 0) { greet(); greeted = true; }
    if (!isMobile()) input.focus(); // no celular, não abrir o teclado sozinho
  }
  function closeChat() { panel.classList.remove("open"); }
  function toggleChat() {
    if (panel.classList.contains("open")) closeChat(); else openChat();
  }
  // expõe para o item de menu "Falar com IA"
  window.tvpOpenChat = openChat;

  panel.querySelector(".tvp-x").addEventListener("click", closeChat);
  sendBtn.addEventListener("click", send);
  input.addEventListener("keydown", function (e) {
    if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); send(); }
  });

  // ---------- arrastar o botão ----------
  var drag = null;
  btn.addEventListener("pointerdown", function (e) {
    drag = { x: e.clientX, y: e.clientY, moved: false, rect: btn.getBoundingClientRect() };
    try { btn.setPointerCapture(e.pointerId); } catch (_) {}
  });
  btn.addEventListener("pointermove", function (e) {
    if (!drag) return;
    var dx = e.clientX - drag.x, dy = e.clientY - drag.y;
    if (!drag.moved && Math.abs(dx) + Math.abs(dy) > 5) {
      drag.moved = true;
      btn.classList.add("tvp-dragging");
    }
    if (drag.moved) {
      applyPos(
        clamp(drag.rect.left + dx, 4, window.innerWidth - btn.offsetWidth - 4),
        clamp(drag.rect.top + dy, 4, window.innerHeight - btn.offsetHeight - 4)
      );
    }
  });
  btn.addEventListener("pointerup", function (e) {
    if (!drag) return;
    var wasDrag = drag.moved;
    drag = null;
    btn.classList.remove("tvp-dragging");
    try { btn.releasePointerCapture(e.pointerId); } catch (_) {}
    if (wasDrag) {
      savePos();
      if (panel.classList.contains("open")) positionPanel();
    } else {
      toggleChat();
    }
  });

  window.addEventListener("resize", function () {
    // mantém o botão dentro da tela e reposiciona o painel se aberto
    var r = btn.getBoundingClientRect();
    if (btn.style.left) {
      applyPos(
        clamp(r.left, 4, window.innerWidth - btn.offsetWidth - 4),
        clamp(r.top, 4, window.innerHeight - btn.offsetHeight - 4)
      );
    }
    if (panel.classList.contains("open")) positionPanel();
  });

  // teclado do celular abre/fecha -> reajusta o painel à área visível
  if (window.visualViewport) {
    var onVV = function () { if (panel.classList.contains("open") && isMobile()) fitMobile(); };
    window.visualViewport.addEventListener("resize", onVV);
    window.visualViewport.addEventListener("scroll", onVV);
  }

  function applyPos(l, t) {
    btn.style.left = l + "px";
    btn.style.top = t + "px";
    btn.style.right = "auto";
    btn.style.bottom = "auto";
  }
  function savePos() {
    try {
      localStorage.setItem(PKEY, JSON.stringify({ left: parseInt(btn.style.left, 10), top: parseInt(btn.style.top, 10) }));
    } catch (_) {}
  }
  function applySavedPos() {
    try {
      var p = JSON.parse(localStorage.getItem(PKEY) || "null");
      if (p && typeof p.left === "number") {
        applyPos(
          clamp(p.left, 4, window.innerWidth - 64),
          clamp(p.top, 4, window.innerHeight - 64)
        );
      }
    } catch (_) {}
  }

  function isMobile() { return window.innerWidth <= 480; }

  // posiciona o painel: bottom-sheet no celular, perto do botão no desktop
  function positionPanel() {
    if (isMobile()) { fitMobile(); return; }
    // desktop: limpa estilos do modo mobile e posiciona perto do botão
    panel.style.width = ""; panel.style.maxWidth = "";
    panel.style.height = ""; panel.style.maxHeight = "";
    var b = btn.getBoundingClientRect();
    var pw = panel.offsetWidth || 370;
    var ph = panel.offsetHeight || 520;
    var gap = 12;
    var left = b.right - pw;
    if (left < 8) left = b.left;
    left = clamp(left, 8, window.innerWidth - pw - 8);
    var top = b.top - ph - gap;
    if (top < 8) top = b.bottom + gap;
    top = clamp(top, 8, window.innerHeight - ph - 8);
    panel.style.left = left + "px";
    panel.style.top = top + "px";
    panel.style.right = "auto";
    panel.style.bottom = "auto";
  }

  // mobile: ocupa só a área VISÍVEL (acima do teclado) via visualViewport,
  // então não estoura a tela quando o teclado abre.
  function fitMobile() {
    var vv = window.visualViewport;
    var h = vv ? vv.height : window.innerHeight;
    var offTop = vv ? vv.offsetTop : 0;
    panel.style.left = "0";
    panel.style.right = "auto";
    panel.style.width = "100vw";
    panel.style.maxWidth = "100vw";
    panel.style.height = h + "px";
    panel.style.maxHeight = "none";
    panel.style.top = offTop + "px";
    panel.style.bottom = "auto";
  }

  // ---------- saudação ----------
  // Busca robusta do nome do usuário logado (o ASP.NET pode prefixar o ID do
  // controle runat=server, ex.: ctl00_spnUserName). Tenta várias estratégias.
  function getUserName() {
    var elName =
      document.getElementById("spnUserName") ||
      document.querySelector('[id$="spnUserName"], [id*="spnUserName"]') ||
      document.querySelector(".user-name");
    if (!elName) return "";
    var full = (elName.textContent || "").trim().replace(/\s+/g, " ");
    return full ? full.split(" ")[0] : "";
  }

  function greet() {
    var nome = getUserName();
    var ola = nome ? "Olá, " + nome + "! " : "Olá! ";
    addBubble("bot", ola + "Sou o Agente de IA do painel. Posso explicar como usar as funções, onde encontrar cada coisa e como configurar. Em que posso ajudar?");
  }

  // ---------- enviar / streaming ----------
  function send() {
    if (streaming) return;
    var text = (input.value || "").trim();
    if (!text) return;
    input.value = "";
    addBubble("user", text);
    history.push({ role: "user", content: text });

    streaming = true;
    sendBtn.disabled = true;
    var botEl = addBubble("bot", "");
    botEl.innerHTML = '<span class="tvp-typing">digitando…</span>';
    var acc = "";

    streamRequest(text, function (delta) {
      acc += delta;
      botEl.innerHTML = render(stripVideo(acc));
      body.scrollTop = body.scrollHeight;
    }, function (err) {
      streaming = false;
      sendBtn.disabled = false;
      if (err) {
        botEl.innerHTML = render(acc ? stripVideo(acc) : "Desculpe, ocorreu um erro. Tente novamente.");
      } else {
        var videoUrl = extractVideo(acc);
        var clean = stripVideo(acc);
        botEl.innerHTML = render(clean);
        if (videoUrl) addVideoButton(botEl, videoUrl);
        history.push({ role: "assistant", content: clean });
      }
      body.scrollTop = body.scrollHeight;
    });
  }

  function streamRequest(message, onDelta, onEnd) {
    fetch(API, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ message: message, history: history.slice(0, -1), sessionId: SESSION_ID }),
    }).then(function (resp) {
      if (!resp.ok || !resp.body) throw new Error("HTTP " + resp.status);
      var reader = resp.body.getReader();
      var dec = new TextDecoder();
      var buf = "";
      function pump() {
        return reader.read().then(function (r) {
          if (r.done) { onEnd(null); return; }
          buf += dec.decode(r.value, { stream: true });
          var parts = buf.split("\n\n");
          buf = parts.pop();
          for (var i = 0; i < parts.length; i++) {
            var evMatch = /event: (\w+)/.exec(parts[i]);
            var dataMatch = /data: (.*)/.exec(parts[i]);
            if (!evMatch || !dataMatch) continue;
            var ev = evMatch[1];
            var data;
            try { data = JSON.parse(dataMatch[1]); } catch (e) { continue; }
            if (ev === "delta") onDelta(data.text || "");
            else if (ev === "error") { onEnd(new Error(data.message || "erro")); return; }
            else if (ev === "done") { onEnd(null); return; }
          }
          return pump();
        });
      }
      return pump();
    }).catch(function (e) { onEnd(e); });
  }

  // ---------- vídeo tutorial ----------
  function extractVideo(text) {
    var m = /\[\[VIDEO:([^\]]+)\]\]/.exec(String(text));
    return m ? m[1].trim() : null;
  }
  function stripVideo(text) {
    return String(text)
      .replace(/\[\[VIDEO:[^\]]*\]\]/g, "")  // token completo
      .replace(/\[\[[^\]]*$/, "")            // token parcial no fim (durante o streaming)
      .replace(/\n{3,}/g, "\n\n")
      .replace(/\s+$/, "");
  }
  function openTutorial(url) {
    try { closeChat(); } catch (_) {}
    try {
      if (typeof window.OpenLink === "function") { window.OpenLink(url); return; } // igual aos Tutoriais
      var f = document.getElementById("content");
      if (f && f.tagName === "IFRAME") { f.src = url; return; }
    } catch (_) {}
    window.open(url, "_blank"); // fallback (ex.: página de teste)
  }
  function addVideoButton(bubbleEl, url) {
    var b = document.createElement("button");
    b.className = "tvp-video-btn";
    b.innerHTML = "&#9654; Ver tutorial";
    b.addEventListener("click", function () { openTutorial(url); });
    bubbleEl.appendChild(b);
  }

  // ---------- markdown leve ----------
  function render(t) {
    var esc = String(t).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var lines = esc.split("\n");
    var html = "", inList = false;
    function closeList() { if (inList) { html += "</ul>"; inList = false; } }
    var inline = function (s) {
      return s.replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>").replace(/`([^`]+)`/g, "<code>$1</code>");
    };
    for (var i = 0; i < lines.length; i++) {
      var ln = lines[i];
      var h = /^\s*(#{1,6})\s+(.*)$/.exec(ln);
      if (h) { closeList(); html += "<div class='tvp-h'>" + inline(h[2]) + "</div>"; continue; }
      if (/^\s*([-*_])\1{2,}\s*$/.test(ln)) { closeList(); html += "<hr>"; continue; }
      ln = ln.replace(/^\s*&gt;\s?/, "");
      if (/^\s*(?:[-•*]|\d+[.)])\s+/.test(ln)) {
        if (!inList) { html += "<ul>"; inList = true; }
        html += "<li>" + inline(ln.replace(/^\s*(?:[-•*]|\d+[.)])\s+/, "")) + "</li>";
      } else {
        closeList();
        if (ln.trim() === "") { html += "<br>"; }
        else { html += inline(ln) + (i < lines.length - 1 ? "<br>" : ""); }
      }
    }
    closeList();
    return html;
  }

  function addBubble(who, text) {
    var wrap = el("div", "tvp-msg " + who);
    var b = el("div", "tvp-bubble");
    b.innerHTML = text ? render(text) : "";
    wrap.appendChild(b);
    body.appendChild(wrap);
    body.scrollTop = body.scrollHeight;
    return b;
  }

  // ---------- "Falar com IA" no menu Suporte ----------
  function injectSuporteItem() {
    var groups = document.querySelectorAll(".sidebar-dropdown");
    for (var i = 0; i < groups.length; i++) {
      var head = groups[i].querySelector(":scope > a span") || groups[i].querySelector("a span");
      if (!head) continue;
      if ((head.textContent || "").trim().toLowerCase() === "suporte") {
        var sub = groups[i].querySelector("ul.sidebar-submenu");
        if (!sub || sub.querySelector(".tvp-menu-item")) return true; // já injetado
        var li = el("li", "tvp-menu-item");
        var a = el("a", null, { href: "#" });
        a.innerHTML = '<i class="fas fa-robot"></i> Falar com IA - BETA';
        a.addEventListener("click", function (e) {
          e.preventDefault(); e.stopPropagation(); openChat();
        });
        li.appendChild(a);
        sub.appendChild(li);
        return true;
      }
    }
    return false;
  }
  // o menu é renderizado por Vue após carregar; tenta por alguns segundos
  var tries = 0;
  var iv = setInterval(function () {
    if (injectSuporteItem() || ++tries > 25) clearInterval(iv);
  }, 400);

  // ---------- util ----------
  function getSessionId() {
    var id = "";
    try { id = sessionStorage.getItem(SKEY) || ""; } catch (_) {}
    if (!id) {
      if (window.crypto && crypto.randomUUID) id = crypto.randomUUID();
      else id = "s-" + Math.random().toString(36).slice(2) + "-" + (new Date().getTime());
      try { sessionStorage.setItem(SKEY, id); } catch (_) {}
    }
    return id;
  }
  function clamp(v, min, max) { return v < min ? min : (v > max ? max : v); }
  function el(tag, cls, attrs) {
    var e = document.createElement(tag);
    if (cls) e.className = cls;
    if (attrs) for (var k in attrs) e.setAttribute(k, attrs[k]);
    return e;
  }
})();
