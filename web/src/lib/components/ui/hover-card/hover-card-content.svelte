<script lang="ts">
  import { LinkPreview as HoverCardPrimitive } from "bits-ui";
  import { cn } from "$lib/utils.js";

  let {
    ref = $bindable(null),
    class: className,
    align = "center",
    sideOffset = 4,
    portalProps,
    ...restProps
  }: HoverCardPrimitive.ContentProps & {
    portalProps?: HoverCardPrimitive.PortalProps;
  } = $props();
</script>

<HoverCardPrimitive.Portal {...portalProps}>
  <HoverCardPrimitive.Content
    bind:ref
    {align}
    {sideOffset}
    class={cn(
      "z-50 w-64 rounded-md border border-stone-700 bg-stone-800 p-4 shadow-lg outline-none",
      "data-[state=open]:animate-in data-[state=closed]:animate-out",
      "data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0",
      "data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95",
      className
    )}
    {...restProps}
  />
</HoverCardPrimitive.Portal>
