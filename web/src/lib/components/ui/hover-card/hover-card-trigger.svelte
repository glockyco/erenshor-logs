<script lang="ts">
  import { LinkPreview as HoverCardPrimitive } from "bits-ui";

  let {
    ref = $bindable(null),
    class: className,
    ...restProps
  }: HoverCardPrimitive.TriggerProps = $props();
</script>

<HoverCardPrimitive.Trigger bind:ref class={className} {...restProps} />
