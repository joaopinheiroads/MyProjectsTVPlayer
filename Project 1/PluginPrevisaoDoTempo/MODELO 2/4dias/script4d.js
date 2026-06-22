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
function pegarPrimeiroItem(n) {
  return n[0];
}
function alterarBackgroundColor(n, t) {
  const i = document.querySelector(".tempo-container");
  i &&
    ((i.style.backgroundColor = n), (document.body.style.backgroundImage = t));
}
function alterarBackgroundColorDias(n) {
  const t = document.querySelector(".tempo-container");
  t && (t.style.backgroundColor = n);
}
function ChangeText(n, t, i, r) {
  for (var u = 0; u < n.length; u++) n[u].innerHTML = idioma == t ? i : r;
}
function ChangeTextUnit(n, t, i, r) {
  for (var u = 0; u < n.length; u++) n[u].innerHTML = unidadeTemp == t ? i : r;
}
function backgroundByClimaCodeHDRetrato(n) {
  switch (n) {
    case 0:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/tornadoVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 50%)");
      console.log("Está com tornado");
      break;
    case 3:
    case 4:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/chuvaForteVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 30%)");
      console.log("Está com tempestade forte e chuva forte");
      break;
    case 5:
    case 6:
    case 18:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/pancadaVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 60%)");
      console.log(
        "Está com Pancadas de chuva irregulares, Tempestade fraca e granizo"
      );
      break;
    case 9:
    case 11:
    case 12:
    case 40:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/chuviscoVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 30%)");
      console.log(
        "Está com Chuvisco, Chuva fraca, Chuva moderada e Pancadas de chuva irregulares"
      );
      break;
    case 10:
    case 17:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/granizoVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 40%)");
      console.log("Está com Pancada de granizo/neve e chuva congelante");
      break;
    case 14:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/neveLeveVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 40%)");
      console.log("Está com pancada leve de neve");
      break;
    case 16:
    case 46:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/NeveVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 50%)");
      console.log("Está com neve e pancada de neve");
      break;
    case 19:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/poeiraVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 4  0%)");
      console.log("Está com areia, poeira e redemoinhos");
      break;
    case 20:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/neblinaVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 30%)");
      console.log("Está com névoa e neblina");
      break;
    case 21:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/neblinaSecaVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 40%)");
      console.log("Está com neblina seca");
      break;
    case 22:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/fumacaVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 50%)");
      console.log("Está com fumaça");
      break;
    case 23:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.2)",
        "url('../assets/rajadaVentoVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 40%)");
      console.log("Está com rajadas de vento");
      break;
    case 26:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.3)",
        "url('../assets/nuvensFragVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 50%)");
      console.log("Está com nuvens fragmentadas");
      break;
    case 32:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/ceuLimpoVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 50%)");
      console.log("Está com céu limpo");
      break;
    case 37:
    case 45:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/chuvaLeveVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 50%)");
      console.log("Está com tempestade irregular e tempestade com chuva leve");
      break;
    case 41:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/pancadaNeveVertiHD.jpg')"
      );
      alterarBackgroundColorDias("rgba(10, 10, 10, 60%)");
      console.log("Está com pancada forte de neve");
      break;
    case 44:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.2)",
        "url('../assets/poucasNuvensVertiHD.jpg')"
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
        "url('../assets/tornadoVertiFH.jpg')"
      );
      console.log("Está com tornado");
      break;
    case 3:
    case 4:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/chuvaForteVertiFH.jpg')"
      );
      console.log("Está com tempestade forte e chuva forte");
      break;
    case 5:
    case 6:
    case 18:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/pancadaVertiFH.jpg')"
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
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/chuviscoVertiFH.jpg')"
      );
      console.log(
        "Está com Chuvisco, Chuva fraca, Chuva moderada e Pancadas de chuva irregulares"
      );
      break;
    case 10:
    case 17:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/granizoVertiFH.jpeg')"
      );
      console.log("Está com Pancada de granizo/neve e chuva congelante");
      break;
    case 14:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/neveLeveVertiFH.jpg')"
      );
      console.log("Está com pancada leve de neve");
      break;
    case 16:
    case 46:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/NeveVertiFH.jpg')"
      );
      console.log("Está com neve e pancada de neve");
      break;
    case 19:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/poeiraVertiFH.jpg')"
      );
      console.log("Está com areia, poeira e redemoinhos");
      break;
    case 20:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/neblinaVertiFH.jpg')"
      );
      console.log("Está com névoa e neblina");
      break;
    case 21:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/neblinaSecaVertiFH.jpg')"
      );
      console.log("Está com neblina seca");
      break;
    case 22:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/fumacaVertiFH.jpg')"
      );
      console.log("Está com fumaça");
      break;
    case 23:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.2)",
        "url('../assets/rajadaVentoVertiFH.jpg')"
      );
      console.log("Está com rajadas de vento");
      break;
    case 26:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.3)",
        "url('../assets/nuvensFragVertiFH.jpg')"
      );
      console.log("Está com nuvens fragmentadas");
      break;
    case 32:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/ceuLimpoVertiFH.jpg')"
      );
      console.log("Está com céu limpo");
      break;
    case 37:
    case 45:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/chuvaLeveVertiFH.jpg')"
      );
      console.log("Está com tempestade irregular e tempestade com chuva leve");
      break;
    case 41:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/pancadaNeveVertiFH.jpg')"
      );
      console.log("Está com pancada forte de neve");
      break;
    case 44:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.2)",
        "url('../assets/poucasNuvensVertiFH.jpg')"
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
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/tornadoHoriHD.jpg')"
      );
      console.log("Está com tornado");
      break;
    case 3:
    case 4:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/chuvaForteHoriHD.jpg')"
      );
      console.log("Está com tempestade forte e chuva forte");
      break;
    case 5:
    case 6:
    case 18:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/pancadaHoriHD.jpg')"
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
        "rgba(10, 10, 10, 0.6)",
        "url('../assets/chuviscoHoriHD.jpg')"
      );
      console.log(
        "Está com Chuvisco, Chuva fraca, Chuva moderada e Pancadas de chuva irregulares"
      );
      break;
    case 10:
    case 17:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/granizoHoriHD.jpg')"
      );
      console.log("Está com Pancada de granizo/neve e chuva congelante");
      break;
    case 14:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/neveLeveHoriHD.jpg')"
      );
      console.log("Está com pancada leve de neve");
      break;
    case 16:
    case 46:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/nevePancadaHoriHD.jpg')"
      );
      console.log("Está com neve e pancada de neve");
      break;
    case 19:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/poeiraHoriHD.jpg')"
      );
      console.log("Está com areia, poeira e redemoinhos");
      break;
    case 20:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/neblinaHoriHD.jpg')"
      );
      console.log("Está com névoa e neblina");
      break;
    case 21:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/NeblinaSecaHoriHD.jpg')"
      );
      console.log("Está com neblina seca");
      break;
    case 22:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/fumacaHoriHD.jpg')"
      );
      console.log("Está com fumaça");
      break;
    case 23:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/vendavalHoriHD.jpg')"
      );
      console.log("Está com rajadas de vento");
      break;
    case 26:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.4)",
        "url('../assets/nuvensFragHoriHD.jpg')"
      );
      console.log("Está com nuvens fragmentadas");
      break;
    case 32:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/ceuLimpoHoriHD.jpg')"
      );
      console.log("Está com céu limpo");
      break;
    case 37:
    case 45:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/chuvaLeveHoriHD.jpg')"
      );
      console.log("Está com tempestade irregular e tempestade com chuva leve");
      break;
    case 41:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4 )",
        "url('../assets/neveForteHoriHD.jpg')"
      );
      console.log("Está com pancada forte de neve");
      break;
    case 44:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/poucasNuvensHoriHD.jpg')"
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
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/tornadoHoriFH.jpg')"
      );
      console.log("Está com tornado");
      break;
    case 3:
    case 4:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/chuvaForteHoriFH.jpg')"
      );
      console.log("Está com tempestade forte e chuva forte");
      break;
    case 5:
    case 6:
    case 18:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.5)",
        "url('../assets/pancadaHoriFH.jpg')"
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
        "rgba(10, 10, 10, 0.6)",
        "url('../assets/chuviscoHoriFH.jpg')"
      );
      console.log(
        "Está com Chuvisco, Chuva fraca, Chuva moderada e Pancadas de chuva irregulares"
      );
      break;
    case 10:
    case 17:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/granizoHoriFH.jpg')"
      );
      console.log("Está com Pancada de granizo/neve e chuva congelante");
      break;
    case 14:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/neveLeveHoriFH.jpg')"
      );
      console.log("Está com pancada leve de neve");
      break;
    case 16:
    case 46:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/nevePancadaHoriFH.jpg')"
      );
      console.log("Está com neve e pancada de neve");
      break;
    case 19:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/poeiraHoriFH.jpg')"
      );
      console.log("Está com areia, poeira e redemoinhos");
      break;
    case 20:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/neblinaHoriFH.jpg')"
      );
      console.log("Está com névoa e neblina");
      break;
    case 21:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/NeblinaSecaHoriFH.jpg')"
      );
      console.log("Está com neblina seca");
      break;
    case 22:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/fumacaHoriFH.jpg')"
      );
      console.log("Está com fumaça");
      break;
    case 23:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/vendavalHoriFH.jpg')"
      );
      console.log("Está com rajadas de vento");
      break;
    case 26:
      alterarBackgroundColor(
        "rgba(0, 0, 0, 0.4)",
        "url('../assets/nuvensFragHoriFH.jpg')"
      );
      console.log("Está com nuvens fragmentadas");
      break;
    case 32:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/ceuLimpoHoriFH.jpg')"
      );
      console.log("Está com céu limpo");
      break;
    case 37:
    case 45:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.3)",
        "url('../assets/chuvaLeveHoriFH.jpg')"
      );
      console.log("Está com tempestade irregular e tempestade com chuva leve");
      break;
    case 41:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/neveForteHoriFH.jpg')"
      );
      console.log("Está com pancada forte de neve");
      break;
    case 44:
      alterarBackgroundColor(
        "rgba(10, 10, 10, 0.4)",
        "url('../assets/poucasNuvensHoriFH.jpg')"
      );
      console.log("Está com poucas nuvens e nuvens dispersas");
  }
  document.body.style.backgroundSize = "cover";
  document.body.style.backgroundPosition = "center";
}
function orientacaoTelaRetrato() {
  window.matchMedia("(orientation: portrait)").matches &&
  window.innerWidth <= 720 &&
  window.innerHeight <= 1280
    ? (backgroundByClimaCodeHDRetrato(parseInt(yCode)),
      console.log("Tela no modo retrato e HD"))
    : window.matchMedia("(orientation: portrait)").matches &&
      (window.innerWidth > 1280 || window.innerHeight > 720) &&
      (backgroundByClimaCodeFHRetrato(parseInt(yCode)),
      console.log("Tela no modo retrato e Full HD!"));
}
function orientacaoTelaPaisagem() {
  if (
    window.matchMedia("(orientation: landscape)").matches &&
    window.innerWidth <= 1280 &&
    window.innerHeight <= 720
  ) {
    backgroundByClimaCodeHDPaisagem(parseInt(yCode));
    const n = document.querySelector(".dias");
    n.style.boxShadow = "unset";
    console.log("Tela em modo paisagem HD");
  } else if (
    window.matchMedia("(orientation: landscape)").matches &&
    (window.innerWidth > 1280 || window.innerHeight > 720)
  ) {
    backgroundByClimaCodeFHPaisagem(parseInt(yCode));
    const n = document.querySelector(".dias");
    n.style.boxShadow = "unset";
    console.log("Tela em modo paisagem Full HD");
  }
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
  function r(n, t, i) {
    let r = parseInt(n.slice(1, 3), 16),
      u = parseInt(n.slice(3, 5), 16),
      f = parseInt(n.slice(5, 7), 16);
    return (
      (r = Math.floor(r * (1 - t))),
      (u = Math.floor(u * (1 - t))),
      (f = Math.floor(f * (1 - t))),
      `rgba(${r}, ${u}, ${f}, ${i})`
    );
  }
  if (parseInt(modelo) === 2) {
    var n = "#" + options.corBg1,
      t = "#" + options.corBg2,
      u = "#" + options.corFonte;
    if (n && t && /^#[0-9A-Fa-f]{6}$/.test(n) && /^#[0-9A-Fa-f]{6}$/.test(t)) {
      const f = document.querySelector(".tempo-container");
      if (f) {
        document.body.style.background = `linear-gradient(${i(
          direcao
        )}, ${n}, ${t})`;
        document.body.style.color = u;
        console.log(`Cor de fundo alterada para: ${n}, ${t}`);
        
      } else console.log("Elemento .tempo-container não encontrado.");
    } else console.log("Parâmetro corBg1 inválido ou não fornecido.");
  }
}
function alteracaoModelo() {
  var n, t;
  console.log("Entrando em alteracaoModelo ");
  n = parseInt(modelo);
  switch (n) {
    case 1:
      orientacaoTelaRetrato();
      orientacaoTelaPaisagem();
      alterarBackgroundColorDias("rgba(0, 0, 0, 30%)");
      console.log("Modelo 1");
      break;
    case 2:
      console.log("Modelo 2");
      applyCustomColorFromUrl();
      alterarBackgroundColorDias("rgba(0, 0, 0, 30%)");
      break;
    case 3:
      console.log("Modelo 3");
      document.body.style.background = "";
      document.body.style.backgroundImage =
        "url('../ImagemFundo/" + options.custombg + "')";
      document.body.style.backgroundSize = "cover";
      document.body.style.backgroundPosition = "center";
      document.body.style.backgroundRepeat = "no-repeat";
      t = "#" + options.corFonte;
      document.body.style.color = t;
      alterarBackgroundColorDias("rgba(0, 0, 0, 30%)");
  }
}
var MyDate = (function () {
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
            "Segunda",
            "Terça",
            "Quarta",
            "Quinta",
            "Sexta",
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
  })(),
  cidade,
  ycodes,
  maxarr,
  minarr,
  climaarr,
  dtarr,
  modelo,
  idioma,
  unidadeTemp,
  direcao,
  options,
  i,
  y_class_code,
  currentData;
options = parseQuery(window.location.search.substring(1));
cidade = options.cidade;
ycodes = options.yCode.split(",");
maxarr = options.max.split(",");
minarr = options.min.split(",");
climaarr = options.clima.split(",");
dtarr = options.dt.split(",");
modelo = options.modelo;
idioma = options.idioma;
unidadeTemp = options.unidadeTemp;
direcao = options.direcao;
console.log("ycodes: " + ycodes);
const yCode = pegarPrimeiroItem(ycodes);
for (
  console.log(yCode),
    document.getElementById("cidade").innerHTML += " " + cidade,
    ChangeTextUnit(document.getElementsByClassName("type-temp"), "c", "C", "F"),
    i = 0;
  i < 4;
  i++
)
  (y_class_code = "wi wi-yahoo-" + ycodes[i]),
    (currentData = new MyDate(dtarr[i], idioma)),
    (document.getElementById("dataAtual" + i).innerHTML =
      currentData.getDiaSemanaCurto() + " " + currentData.getDiaMes()),
    (document.getElementById("condicaoClima" + i).innerHTML = climaarr[i]),
    (document.getElementById("minValue" + i).innerHTML = minarr[i] + "°"),
    (document.getElementById("maxValue" + i).innerHTML = maxarr[i] + "°"),
    (document.getElementById("icone" + i).className = y_class_code);
alteracaoModelo();



function mostrarTamanho() {
      // Captura largura e altura da tela (viewport visível)
      var largura = window.innerWidth;
      var altura = window.innerHeight;

      // Pega o span pelo ID e insere os valores
      var span = document.getElementById("teste");
      span.textContent = "Largura: " + largura + "px, Altura: " + altura + "px";
    }

    // Chama a função ao carregar
    mostrarTamanho();

    // Atualiza também quando a janela for redimensionada
    window.onresize = mostrarTamanho;

