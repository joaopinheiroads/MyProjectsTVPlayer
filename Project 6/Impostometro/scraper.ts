import express from "express";
import type { Request, Response } from "express";
import cors from "cors";
import puppeteer, { Browser } from "puppeteer";
import https from "https";
import fs from "fs";

const app = express();

app.use(cors());

const PORT = 3001;
const URL = "https://impostometro.com.br/";
const UPDATE_INTERVAL = 300000;



interface Cache {
  valor: string;
  atualizadoEm: Date | null;
}

let cache: Cache = {
  valor: "0",
  atualizadoEm: null,
};

// 🔥 BROWSER
let browser: Browser | null = null;

async function initBrowser(): Promise<void> {
  if (!browser) {
    browser = await puppeteer.launch({
      headless: true,
      args: ["--no-sandbox", "--disable-setuid-sandbox"],
    });

  }
}

async function atualizarValor(): Promise<void> {
  if (!browser) {
    console.error("❌ Browser não iniciado");
    return;
  }

  let page = null;

  try {
    page = await browser.newPage();

    await page.goto(URL, {
      waitUntil: "networkidle2",
      timeout: 30000,
    });

    await page.waitForSelector("#counterBrasil", {
      timeout: 15000,
    });

    const valor: string = await page.evaluate(() => {
      const elementos = document.querySelectorAll(
        "#counterBrasil .counter-inside"
      );

      const numeros = Array.from(elementos).map(
        (el) => (el as HTMLElement).innerText.trim()
      );

      const inteiro = numeros.slice(0, -2).join("");
      const decimal = numeros.slice(-2).join("");

      return (
        inteiro.replace(/\B(?=(\d{3})+(?!\d))/g, ".") +
        "," +
        decimal
      );
    });

    cache = {
      valor,
      atualizadoEm: new Date(),
    };

    console.log("✅ Atualizado:", valor);

  } catch (error) {
    console.error(
      "❌ Erro ao atualizar:",
      (error as Error).message
    );
  } finally {
    if (page) {
      await page.close();
    }
  }
}

async function startWorker(): Promise<void> {
  await initBrowser();

  await atualizarValor();

  setInterval(async () => {
    await atualizarValor();
  }, UPDATE_INTERVAL);
}


app.get("/impostometro", (req: Request, res: Response) => {

  res.json(cache);
  
});



  



const httpsOptions = {
  pfx: fs.readFileSync("./cert.pfx"),
  passphrase: "[PASSPHRASE_REMOVIDA]",
};

https.createServer(httpsOptions, app).listen(PORT, async () => {
  await startWorker();
});