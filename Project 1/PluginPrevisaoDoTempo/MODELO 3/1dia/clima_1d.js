function parseQuery(n) {
  var u;
  if (typeof n != "string" || n.length == 0) return {};
  var e = n.split("&"),
    o = e.length,
    f,
    i = {},
    t,
    r;
  for (u = 0; u < o; u++)
    ((f = e[u].split("=")), (t = decodeURIComponent(f[0])), t.length != 0) &&
      ((r = decodeURIComponent(f[1])),
      typeof i[t] == "undefined"
        ? (i[t] = r)
        : i[t] instanceof Array
        ? i[t].push(r)
        : (i[t] = [i[t], r]));
  return i;
}
function splitNumbers(n, t) {
  return n.split(t);
}
function ChangeText(n, t, i, r) {
  for (var u = 0; u < n.length; u++) n[u].innerHTML = idioma == t ? i : r;
}
function ChangeTextUnit(n, t, i, r) {
  for (var u = 0; u < n.length; u++) n[u].innerHTML = unidadeTemp == t ? i : r;
}
function ChangeTextVelocity(n, t, i, r) {
  n.innerHTML = idioma == t ? i : r;
}
function alterarBackgroundColorDois(n) {
  const t = document.querySelector(".tempo-container");
  t && ((t.style.backgroundColor = n), (t.style.backdropFilter = "blur(10px)"));
}
function alterarBackgroundColor(n, t) {
  const i = document.querySelector(".wi");
  i &&
    ((i.style.backgroundColor = n), (document.body.style.backgroundImage = t));
}
function backgroundByClimaCodeHDRetrato(n) {
  switch (n) {
    case 0:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.4)",
        "url('../assets/bg_tornado_v_HD.png')",
        "blur(50px)"
      );
      console.log("Está com tornado");
      break;
    case 3:
    case 4:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_chuvaforte_v_HD.png')"
      );
      console.log("Está com tempestade forte e chuva forte");
      break;
    case 5:
    case 6:
    case 18:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_tempestadeirregular_v_HD.png')"
      );
      console.log(
        "Está com Pancadas de chuva irregulares, Tempestade fraca e granizo"
      );
      break;
    case 9:
    case 11:
    case 12:
    case 40:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_chuvafraca_v_HD.png')"
      );
      console.log(
        "Está com Chuvisco, Chuva fraca, Chuva moderada e Pancadas de chuva irregulares"
      );
      break;
    case 10:
    case 17:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_chuvadegranizo_v_HD.png')"
      );
      console.log("Está com Pancada de granizo/neve e chuva congelante");
      break;
    case 14:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_pancadalevedeneve_v_HD.png')"
      );
      console.log("Está com pancada leve de neve");
      break;
    case 16:
    case 46:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.6)",
        "url('../assets/bg_neve_v_HD.png')"
      );
      console.log("Está com neve e pancada de neve");
      break;
    case 19:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_poeira_v_HD.png')"
      );
      console.log("Está com areia, poeira e redemoinhos");
      break;
    case 20:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_neblina_v_HD.png')"
      );
      console.log("Está com névoa e neblina");
      break;
    case 21:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_neblinaseca_v_HD.png')"
      );
      console.log("Está com neblina seca");
      break;
    case 22:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_fumaca_v_HD.png')"
      );
      console.log("Está com fumaça");
      break;
    case 23:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.2)",
        "url('../assets/bg_rajadasdevento_v_HD.png')"
      );
      console.log("Está com rajadas de vento");
      break;
    case 26:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.4)",
        "url('../assets/bg_nuvensfragmentadas_v_HD.png')"
      );
      console.log("Está com nuvens fragmentadas");
      break;
    case 32:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.3)",
        "url('../assets/bg_ceulimpo_v_HD.png')"
      );
      console.log("Está com céu limpo");
      break;
    case 37:
    case 45:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_tempestadefraca_v_HD.png')"
      );
      console.log("Está com tempestade irregular e tempestade com chuva leve");
      break;
    case 41:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_pancadafortedeneve_v_HD.png')"
      );
      console.log("Está com pancada forte de neve");
      break;
    case 44:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_poucasnuvens_v_HD.png')"
      );
      console.log("Está com poucas nuvens e nuvens dispersas");
  }
  document.body.style.backgroundSize = "cover";
  document.body.style.backgroundPosition = "center";
}
function backgroundByClimaCodeFHRetrato(n) {
  switch (n) {
    case 0:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_tornado_v.png')"
      );
      console.log("Está com tornado");
      break;
    case 3:
    case 4:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.6)",
        "url('../assets/bg_chuvaforte_v.png')"
      );
      console.log("Está com tempestade forte e chuva forte");
      break;
    case 5:
    case 6:
    case 18:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.8)",
        "url('../assets/bg_tempestadeirregular_v.png')"
      );
      console.log(
        "Está com Pancadas de chuva irregulares, Tempestade fraca e granizo"
      );
      break;
    case 9:
    case 11:
    case 12:
    case 40:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.7)",
        "url('../assets/bg_chuvafraca_v.png')"
      );
      console.log(
        "Está com Chuvisco, Chuva fraca, Chuva moderada e Pancadas de chuva irregulares"
      );
      break;
    case 10:
    case 17:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.7)",
        "url('../assets/bg_chuvadegranizo_v.png')"
      );
      console.log("Está com Pancada de granizo/neve e chuva congelante");
      break;
    case 14:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.7)",
        "url('../assets/bg_pancadalevedeneve_v.png')"
      );
      console.log("Está com pancada leve de neve");
      break;
    case 16:
    case 46:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.7)",
        "url('../assets/bg_neve_v.png')"
      );
      console.log("Está com neve e pancada de neve");
      break;
    case 19:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.7)",
        "url('../assets/bg_poeira_v.png')"
      );
      console.log("Está com areia, poeira e redemoinhos");
      break;
    case 20:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.6)",
        "url('../assets/bg_neblina_v.png')"
      );
      console.log("Está com névoa e neblina");
      break;
    case 21:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.7)",
        "url('../assets/bg_neblinaseca_v.png')"
      );
      console.log("Está com neblina seca");
      break;
    case 22:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.7)",
        "url('../assets/bg_fumaca_v.png')"
      );
      console.log("Está com fumaça");
      break;
    case 23:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.6)",
        "url('../assets/bg_rajadasdevento_v.png')"
      );
      console.log("Está com rajadas de vento");
      break;
    case 26:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.6)",
        "url('../assets/bg_nuvensfragmentadas_v.png')"
      );
      console.log("Está com nuvens fragmentadas");
      break;
    case 32:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_ceulimpo_v.png')"
      );
      console.log("Está com céu limpo");
      break;
    case 37:
    case 45:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_tempestadefraca_v.png')"
      );
      console.log("Está com tempestade irregular e tempestade com chuva leve");
      break;
    case 41:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_pancadafortedeneve_v.png')"
      );
      console.log("Está com pancada forte de neve");
      break;
    case 44:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_poucasnuvens_v_HD.png')"
      );
      console.log("Está com poucas nuvens e nuvens dispersas");
  }
  document.body.style.backgroundSize = "cover";
  document.body.style.backgroundPosition = "center";
}
function backgroundByClimaCodeHDPaisagem(n) {
  switch (n) {
    case 0:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_tornado_h.png')"
      );
      console.log("Está com tornado");
      break;
    case 3:
    case 4:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_chuvaforte_h_HD.png')"
      );
      console.log("Está com tempestade forte e chuva forte");
      break;
    case 5:
    case 6:
    case 18:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_tempestadeirregular_h_HD.png')"
      );
      console.log(
        "Está com Pancadas de chuva irregulares, Tempestade fraca e granizo"
      );
      break;
    case 9:
    case 11:
    case 12:
    case 40:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_chuvafraca_h_HD.png')"
      );
      console.log(
        "Está com Chuvisco, Chuva fraca, Chuva moderada e Pancadas de chuva irregulares"
      );
      break;
    case 10:
    case 17:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_chuvadegranizo_h_HD.png')"
      );
      console.log("Está com Pancada de granizo/neve e chuva congelante");
      break;
    case 14:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_pancadalevedeneve_h_HD.png')"
      );
      console.log("Está com pancada leve de neve");
      break;
    case 16:
    case 46:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_neve_h_HD.png')"
      );
      console.log("Está com neve e pancada de neve");
      break;
    case 19:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_poeira_h_HD.png')"
      );
      console.log("Está com areia, poeira e redemoinhos");
      break;
    case 20:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_neblina_h_HD.png')"
      );
      console.log("Está com névoa e neblina");
      break;
    case 21:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_neblinaseca_h_HD.png')"
      );
      console.log("Está com neblina seca");
      break;
    case 22:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_fumaca_h_HD.png')"
      );
      console.log("Está com fumaça");
      break;
    case 23:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_rajadasdevento_h_HD.png')"
      );
      console.log("Está com rajadas de vento");
      break;
    case 26:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.4)",
        "url('../assets/bg_nuvensfragmentadas_h_HD.png')"
      );
      console.log("Está com nuvens fragmentadas");
      break;
    case 32:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_ceulimpo_h_HD.png')"
      );
      console.log("Está com céu limpo");
      break;
    case 37:
    case 45:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_tempestadefraca_h_HD.png')"
      );
      console.log("Está com tempestade irregular e tempestade com chuva leve");
      break;
    case 41:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4 )",
        "url('../assets/bg_pancadafortedeneve_h_HD.png')"
      );
      console.log("Está com pancada forte de neve");
      break;
    case 44:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.6)",
        "url('../assets/bg_poucasnuvens_h_HD.png')"
      );
      console.log("Está com poucas nuvens e nuvens dispersas");
  }
  document.body.style.backgroundSize = "cover";
  document.body.style.backgroundPosition = "center";
}
function backgroundByClimaCodeFHPaisagem(n) {
  switch (n) {
    case 0:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_tornado_h.png')"
      );
      console.log("Está com tornado");
      break;
    case 3:
    case 4:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_chuvaforte_h.png')"
      );
      console.log("Está com tempestade forte e chuva forte");
      break;
    case 5:
    case 6:
    case 18:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_tempestadeirregular_h.png')"
      );
      console.log(
        "Está com Pancadas de chuva irregulares, Tempestade fraca e granizo"
      );
      break;
    case 9:
    case 11:
    case 12:
    case 40:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_chuvafraca_h.png')"
      );
      console.log(
        "Está com Chuvisco, Chuva fraca, Chuva moderada e Pancadas de chuva irregulares"
      );
      break;
    case 10:
    case 17:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_chuvadegranizo_h.png')"
      );
      console.log("Está com Pancada de granizo/neve e chuva congelante");
      break;
    case 14:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_pancadalevedeneve_h.png')"
      );
      console.log("Está com pancada leve de neve");
      break;
    case 16:
    case 46:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_neve_h.png')"
      );
      console.log("Está com neve e pancada de neve");
      break;
    case 19:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_poeira_h.png')"
      );
      console.log("Está com areia, poeira e redemoinhos");
      break;
    case 20:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_neblina_h.png')"
      );
      console.log("Está com névoa e neblina");
      break;
    case 21:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.7)",
        "url('../assets/bg_neblinaseca_h.png')"
      );
      console.log("Está com neblina seca");
      break;
    case 22:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_fumaca_h.png')"
      );
      console.log("Está com fumaça");
      break;
    case 23:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/bg_rajadasdevento_h.png')"
      );
      console.log("Está com rajadas de vento");
      break;
    case 26:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.4)",
        "url('../assets/bg_nuvensfragmentadas_h.png')"
      );
      console.log("Está com nuvens fragmentadas");
      break;
    case 32:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_ceulimpo_h.png')"
      );
      console.log("Está com céu limpo");
      break;
    case 37:
    case 45:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/bg_tempestadefraca_h.png')"
      );
      console.log("Está com tempestade irregular e tempestade com chuva leve");
      break;
    case 41:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/bg_pancadafortedeneve_h.png')"
      );
      console.log("Está com pancada forte de neve");
      break;
    case 44:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.6)",
        "url('../assets/bg_poucasnuvens_h.png')"
      );
      console.log("Está com poucas nuvens e nuvens dispersas");
  }
  document.body.style.backgroundSize = "cover";
  document.body.style.backgroundPosition = "center";
}
function applyCustomColorFromUrl() {
  function i(n) {
    switch (n) {
      case "h":
        return "to right";
      case "v":
        return "to bottom";
      default:
        return "to right";
    }
  }
  if (parseInt(modelo) === 5) {
    var n = "#" + options.corBg1,
      t = "#" + options.corBg2,
      r = "#" + options.corFonte;
    if ((n, t && /^#[0-9A-Fa-f]{6}$/.test(n) && /^#[0-9A-Fa-f]{6}$/.test(t))) {
      const u = document.querySelector(".tempo-container");
      u
        ? ((document.body.style.background = `linear-gradient(${i(
            direcao
          )}, ${n}, ${t})`),
          (document.body.style.color = r),
          console.log(`Cor de fundo alterada para: ${n}, ${t}`))
        : console.log("Elemento .tempo-container não encontrado.");
    } else console.log("Parâmetro corBg1 inválido ou não fornecido.");
  }
}
function orientacaoTelaRetrato() {
  window.matchMedia("(orientation: portrait)").matches &&
  window.innerWidth <= 720 &&
  window.innerHeight <= 1280
    ? (backgroundByClimaCodeHDRetrato(parseInt(options.yCode)),
      console.log("Tela no modo retrato e HD"))
    : window.matchMedia("(orientation: portrait)").matches &&
      (window.innerWidth > 1280 || window.innerHeight > 720) &&
      (backgroundByClimaCodeFHRetrato(parseInt(options.yCode)),
      console.log("Tela no modo retrato e Full HD!"));
}
function orientacaoTelaPaisagem() {
  window.matchMedia("(orientation: landscape)").matches &&
  window.innerWidth <= 1280 &&
  window.innerHeight <= 720
    ? (backgroundByClimaCodeHDPaisagem(parseInt(options.yCode)),
      console.log("Tela em modo paisagem HD"))
    : window.matchMedia("(orientation: landscape)").matches &&
      (window.innerWidth > 1280 || window.innerHeight > 720) &&
      (backgroundByClimaCodeFHPaisagem(parseInt(options.yCode)),
      console.log("Tela em modo paisagem Full HD"));
}
function alterarBackgroundColorDias(n) {
  const t = document.querySelector(".wi");
  t && (t.style.backgroundColor = n);
}
function alteracaoModelo() {
  var n, t;
  console.log("Entrando em alteracaoModelo ");
  n = parseInt(modelo);
  switch (n) {
    case 1:
	case 4:
      orientacaoTelaRetrato();
      orientacaoTelaPaisagem();
      alterarBackgroundColorDias("rgba(0, 0, 0, 30%)");
      console.log("Modelo 1");
      break;
    case 2:
	case 5:
      console.log("Modelo 2");
      applyCustomColorFromUrl();
      alterarBackgroundColorDias("rgba(0, 0, 0, 30%)");
      break;
    case 3:
	case 6:
      console.log("Modelo 3");
      document.body.style.background = "";
      document.body.style.backgroundImage =
        "url('../ImagemFundo/" + options.custombg + "')";
      document.body.style.backgroundSize = "cover";
      document.body.style.backgroundPosition = "center";
      t = "#" + options.corFonte;
      document.body.style.color = t;
      alterarBackgroundColorDias("rgba(0, 0, 0, 30%)");
  }
}
var cidade,
  yCode,
  max,
  min,
  clima,
  modelo,
  minV,
  maxV,
  minU,
  maxU,
  MyDate,
  idioma,
  unidadeTemp,
  unidadeVelocidade,
  direcao,
  options,
  y_class_code,
  currentDate;
(function () {
  function r(n, t, i) {
    var r = document.createEvent("HTMLEvents");
    r.initEvent(n, !0, !1);
    t !== undefined && (r.clientWidth = t);
    i !== undefined && (r.clientHeight = i);
    document.dispatchEvent(r);
  }
  function t() {
    var n = window.innerWidth,
      t = window.innerHeight;
    r("measurechange", n, t);
  }
  function u() {
    n && window.clearTimeout(n);
    n = window.setTimeout(t, 500);
  }
  function f() {
    window.setTimeout(t, 300);
  }
  var n = null,
    i = !1;
  document.addEventListener(
    "show",
    function () {
      i = !0;
    },
    !1
  );
  window.addEventListener("DOMContentLoaded", t, !1);
  window.addEventListener("load", f, !1);
  window.addEventListener("resize", u, !1);
})();
MyDate = (function () {
  function n(n, t) {
    this.idioma = {
      ptbr: {
        nomesMes: [
          "Janeiro",
          "Fevereiro",
          "Março",
          "Abril",
          "Maio",
          "Junho",
          "Julho",
          "Agosto",
          "Setembro",
          "Outubro",
          "Novembro",
          "Dezembro",
        ],
        nomesMesCurto: [
          "Jan",
          "Fev",
          "Mar",
          "Abr",
          "Mai",
          "Jun",
          "Jul",
          "Ago",
          "Set",
          "Out",
          "Nov",
          "Dez",
        ],
        diaSemana: [
          "Domingo",
          "Segunda-feira",
          "Terça-feira",
          "Quarta-feira",
          "Quinta-feira",
          "Sexta-feira",
          "Sábado",
        ],
        diaSemanaCurto: ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"],
      },
      eng: {
        nomesMes: [
          "January",
          "February",
          "March",
          "April",
          "May",
          "June",
          "July",
          "August",
          "September",
          "October",
          "November",
          "December",
        ],
        nomesMesCurto: [
          "Jan",
          "Feb",
          "Mar",
          "Apr",
          "May",
          "Jun",
          "Jul",
          "Aug",
          "Sep",
          "Oct",
          "Nov",
          "Dec",
        ],
        diaSemana: [
          "Sunday",
          "Monday",
          "Tuesday",
          "Wednesday",
          "Thursday",
          "Friday",
          "Saturday",
        ],
        diaSemanaCurto: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"],
      },
    };
    this.currentDate = new Date(n);
    this.language = t;
  }
  return (
    (n.prototype.getMesLongo = function () {
      return this.idioma[this.language].nomesMes[this.currentDate.getMonth()];
    }),
    (n.prototype.getMesCurto = function () {
      return this.idioma[this.language].nomesMesCurto[
        this.currentDate.getMonth()
      ];
    }),
    (n.prototype.getDiaMes = function () {
      return this.currentDate.getDate();
    }),
    (n.prototype.getDiaSemanaLongo = function () {
      return this.idioma[this.language].diaSemana[this.currentDate.getDay()];
    }),
    (n.prototype.getDiaSemanaCurto = function () {
      return this.idioma[this.language].diaSemanaCurto[
        this.currentDate.getDay()
      ];
    }),
    n
  );
})();
options = parseQuery(window.location.search.substring(1));
cidade = options.cidade;
yCode = options.yCode;
max = options.max;
min = options.min;
clima = options.clima;
modelo = options.modelo;
minV = options.minV;
maxV = options.maxV;
minU = options.minU;
maxU = options.maxU;
idioma = options.idioma;
unidadeTemp = options.unidadeTemp;
unidadeVelocidade = options.unidadeVelocidade;
direcao = options.direcao;
y_class_code = "wi wi-yahoo-" + yCode;
currentDate = new MyDate(options.dt, idioma);
document.getElementById("dataAtual").innerHTML =


  currentDate.getDiaSemanaLongo() + " " + currentDate.getDiaMes();



document.getElementById("cidade").innerHTML += " " + cidade;
document.getElementById("condicaoClima").innerHTML = clima;
document.getElementById("minValue").innerHTML = min + "°";
document.getElementById("maxValue").innerHTML = max + "°";
document.getElementById("minValueV").innerHTML = minV;
document.getElementById("maxValueV").innerHTML = maxV;
document.getElementById("icon").className = y_class_code;
document.getElementById("minU").innerHTML = minU;
document.getElementById("maxU").innerHTML = maxU;
ChangeText(document.getElementsByClassName("max-text"), "eng", "high", "máx");
ChangeText(document.getElementsByClassName("min-text"), "eng", "low", "mín");
ChangeTextUnit(document.getElementsByClassName("type-temp"), "c", "C", "F");
ChangeTextVelocity(document.getElementById("kmUm"), "eng", "mph", "km/h");
ChangeTextVelocity(document.getElementById("kmDois"), "eng", "mph", "km/h");
ChangeText(
  document.getElementsByClassName("vento-text"),
  "eng",
  "Wind speed",
  "Velocidade do vento"
);
ChangeText(
  document.getElementsByClassName("umidade-text"),
  "eng",
  "Humidity",
  "Umidade"
);
window.addEventListener("orientationchange", function () {
  alteracaoModelo();
});
alteracaoModelo();



