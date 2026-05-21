import { dotnet } from './_framework/dotnet.js';

const status = document.getElementById('status');
const canvas = document.getElementById('canvas');

if (!(canvas instanceof HTMLCanvasElement)) {
  throw new Error('Missing #canvas element.');
}

try {
  const { getConfig, runMain } = await dotnet
    .withDiagnosticTracing(false)
    .withModuleConfig({ canvas })
    .create();

  const config = getConfig();
  status.textContent = `Running ${config.mainAssemblyName}...`;
  await runMain(config.mainAssemblyName, []);
  status.textContent = `${config.mainAssemblyName} initialized.`;
} catch (error) {
  console.error(error);
  status.textContent = error instanceof Error ? error.stack : String(error);
}
