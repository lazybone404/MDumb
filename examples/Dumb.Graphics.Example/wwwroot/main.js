import { dotnet } from './_framework/dotnet.js';

const status = document.getElementById('status');

try {
  const { getConfig, runMain } = await dotnet
    .withDiagnosticTracing(false)
    .create();

  const config = getConfig();
  status.textContent = `Running ${config.mainAssemblyName}...`;
  await runMain(config.mainAssemblyName, []);
  status.textContent = `${config.mainAssemblyName} completed one offscreen WebGPU frame. Check the browser console for the resource coverage log.`;
} catch (error) {
  console.error(error);
  status.textContent = error instanceof Error ? error.stack : String(error);
}
