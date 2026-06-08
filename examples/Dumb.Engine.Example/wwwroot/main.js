import { dotnet } from './_framework/dotnet.js';

const status = document.getElementById('status');
const canvas = document.getElementById('canvas');

if (!(canvas instanceof HTMLCanvasElement)) {
  throw new Error('Missing #canvas element.');
}

canvas.addEventListener('click', () => canvas.focus());
canvas.addEventListener('pointerdown', () => canvas.focus());
canvas.focus();

try {
  const { getConfig, runMain } = await dotnet
    .withDiagnosticTracing(false)
    .withModuleConfig({ canvas })
    .create();

  const config = getConfig();
  status.textContent = `Running ${config.mainAssemblyName}...`;
  await runMain(config.mainAssemblyName, []);
  status.textContent = `${config.mainAssemblyName} initialized. Click the canvas before testing keyboard, mouse, wheel, audio, and gamepad input.`;
} catch (error) {
  console.error(error);
  status.textContent = error instanceof Error ? error.stack : String(error);
}
