document.addEventListener("DOMContentLoaded", function () {
    const data = window.nuevaVentaData || {};
    const clientes = data.clientes || [];
    const medicamentos = data.medicamentos || [];

    const clienteBusqueda = document.getElementById("clienteBusqueda");
    const clienteBusquedaWrap = document.getElementById("clienteBusquedaWrap");
    const clienteResultados = document.getElementById("clienteResultados");
    const clienteSeleccionado = document.getElementById("clienteSeleccionado");
    const clienteError = document.getElementById("clienteError");
    const clienteId = document.getElementById("Venta_IdCliente");
    const btnConsumidorFinal = document.getElementById("btnConsumidorFinal");
    const btnClienteRegistrado = document.getElementById("btnClienteRegistrado");
    const clienteRapidoForm = document.getElementById("clienteRapidoForm");
    const clienteRapidoError = document.getElementById("clienteRapidoError");
    const btnGuardarClienteRapido = document.getElementById("btnGuardarClienteRapido");
    const modalClienteRapido = bootstrap.Modal.getOrCreateInstance(document.getElementById("modalClienteRapido"));

    const metodoPagoInput = document.getElementById("Venta_MetodoPago");
    const pagoError = document.getElementById("pagoError");
    const clienteConfirmacion = document.getElementById("clienteConfirmacion");
    const pagoConfirmacion = document.getElementById("pagoConfirmacion");

    const medicamentoBusqueda = document.getElementById("medicamentoBusqueda");
    const medicamentoResultados = document.getElementById("medicamentoResultados");
    const medicamentoError = document.getElementById("medicamentoError");
    const agregarDetalle = document.getElementById("agregarDetalle");
    const tabla = document.querySelector("#detalleVenta tbody");
    const detalleVacio = document.getElementById("detalleVacio");
    const detalleError = document.getElementById("detalleError");
    const totalVenta = document.getElementById("totalVenta");
    const totalConfirmacion = document.getElementById("totalConfirmacion");
    const ventaMensajeError = document.getElementById("ventaMensajeError");
    const btnPrepararVenta = document.getElementById("btnPrepararVenta");
    const modalConfirmarVenta = new bootstrap.Modal(document.getElementById("modalConfirmarVenta"));

    let medicamentoSeleccionado = null;
    let clienteActual = null;

    function mostrarErrorGeneral(mensaje) {
        ventaMensajeError.textContent = mensaje;
        ventaMensajeError.classList.remove("d-none");
        ventaMensajeError.scrollIntoView({ behavior: "smooth", block: "center" });
    }

    function limpiarErrorGeneral() {
        ventaMensajeError.textContent = "";
        ventaMensajeError.classList.add("d-none");
    }

    function mostrarValidacion(elemento, mensaje, foco) {
        elemento.textContent = mensaje;
        elemento.classList.remove("d-none");
        if (foco) {
            foco.focus();
            foco.scrollIntoView({ behavior: "smooth", block: "center" });
        }
    }

    function limpiarValidacion(elemento) {
        elemento.textContent = "";
        elemento.classList.add("d-none");
    }

    function limpiarValidaciones() {
        limpiarErrorGeneral();
        limpiarValidacion(clienteError);
        limpiarValidacion(pagoError);
        limpiarValidacion(medicamentoError);
        limpiarValidacion(detalleError);
    }

    function pintarCliente(cliente, esConsumidorFinal) {
        clienteSeleccionado.textContent = `${esConsumidorFinal ? "Consumidor final" : "Cliente"}: ${cliente.razonSocial || "Sin razon social"} | NIT / CI: ${cliente.nit || "-"}`;
        clienteConfirmacion.textContent = cliente.razonSocial || "Sin razon social";
    }

    function seleccionarCliente(cliente, esConsumidorFinal = false) {
        clienteActual = cliente;
        clienteId.value = cliente.id;
        clienteBusqueda.value = esConsumidorFinal ? "" : `${cliente.nit} - ${cliente.razonSocial}`;
        clienteResultados.innerHTML = "";
        pintarCliente(cliente, esConsumidorFinal);
        limpiarValidacion(clienteError);
        limpiarErrorGeneral();
    }

    function buscarConsumidorFinal() {
        return clientes.find(cliente =>
            cliente.esConsumidorFinal ||
            (cliente.nit || "").toLowerCase() === "cf" ||
            (cliente.nit || "") === "0" ||
            (cliente.razonSocial || "").toLowerCase() === "consumidor final");
    }

    function activarModoConsumidorFinal() {
        btnConsumidorFinal.classList.add("active");
        btnClienteRegistrado.classList.remove("active");
        clienteBusquedaWrap.classList.add("d-none");

        const consumidorFinal = buscarConsumidorFinal();
        if (!consumidorFinal) {
            clienteId.value = "";
            clienteActual = null;
            clienteSeleccionado.textContent = "No existe un cliente configurado como consumidor final.";
            mostrarValidacion(clienteError, "No existe un cliente configurado como consumidor final.", btnConsumidorFinal);
            return;
        }

        seleccionarCliente(consumidorFinal, true);
    }

    function activarModoClienteRegistrado() {
        btnClienteRegistrado.classList.add("active");
        btnConsumidorFinal.classList.remove("active");
        clienteBusquedaWrap.classList.remove("d-none");
        clienteBusqueda.focus();
        clienteId.value = "";
        clienteActual = null;
        clienteBusqueda.value = "";
        clienteSeleccionado.textContent = "Busca y selecciona un cliente registrado.";
        clienteConfirmacion.textContent = "Sin cliente";
        limpiarValidacion(clienteError);
        limpiarErrorGeneral();
    }

    function filtrarClientes(texto) {
        return clientes
            .filter(cliente =>
                !cliente.esConsumidorFinal &&
                ((cliente.nit || "").toLowerCase().includes(texto) ||
                (cliente.razonSocial || "").toLowerCase().includes(texto)))
            .slice(0, 8);
    }

    function filtrarMedicamentos(texto) {
        return medicamentos
            .filter(medicamento =>
                (medicamento.nombre || "").toLowerCase().includes(texto) ||
                (medicamento.presentacion || "").toLowerCase().includes(texto))
            .slice(0, 8);
    }

    btnConsumidorFinal.addEventListener("click", activarModoConsumidorFinal);
    btnClienteRegistrado.addEventListener("click", activarModoClienteRegistrado);

    clienteBusqueda.addEventListener("input", function () {
        const texto = this.value.trim().toLowerCase();
        clienteId.value = "";
        clienteActual = null;
        clienteSeleccionado.textContent = "Busca y selecciona un cliente registrado.";
        clienteResultados.innerHTML = "";
        limpiarValidacion(clienteError);

        if (texto.length < 2) return;

        filtrarClientes(texto).forEach(cliente => {
            const item = document.createElement("button");
            item.type = "button";
            item.className = "list-group-item list-group-item-action";
            item.innerHTML = `<strong>${cliente.razonSocial}</strong><br><span class="text-muted small">NIT / CI: ${cliente.nit || "-"}</span>`;
            item.addEventListener("click", () => seleccionarCliente(cliente, false));
            clienteResultados.appendChild(item);
        });
    });

    metodoPagoInput.addEventListener("change", function () {
        pagoConfirmacion.textContent = this.value || "Sin seleccionar";
        limpiarValidacion(pagoError);
        limpiarErrorGeneral();
    });

    clienteRapidoForm.addEventListener("submit", async function (event) {
        event.preventDefault();

        clienteRapidoError.classList.add("d-none");
        btnGuardarClienteRapido.disabled = true;
        btnGuardarClienteRapido.textContent = "Guardando...";

        try {
            const formData = new FormData(clienteRapidoForm);
            const response = await fetch(`${window.location.pathname}?handler=CrearClienteRapido`, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const responseData = await response.json();
            if (!response.ok || !responseData.success) {
                throw new Error(responseData.error || "No se pudo registrar el cliente.");
            }

            const cliente = responseData.cliente;
            if (!clientes.some(item => Number(item.id) === Number(cliente.id))) {
                clientes.push(cliente);
            }

            activarModoClienteRegistrado();
            seleccionarCliente(cliente, false);
            clienteRapidoForm.reset();
            modalClienteRapido.hide();
        } catch (error) {
            clienteRapidoError.textContent = error.message;
            clienteRapidoError.classList.remove("d-none");
        } finally {
            btnGuardarClienteRapido.disabled = false;
            btnGuardarClienteRapido.textContent = "Guardar cliente";
        }
    });

    function seleccionarMedicamento(medicamento) {
        medicamentoSeleccionado = medicamento;
        medicamentoBusqueda.value = `${medicamento.nombre} - ${medicamento.presentacion}`;
        medicamentoResultados.innerHTML = "";
        limpiarValidacion(medicamentoError);
        limpiarErrorGeneral();
    }

    medicamentoBusqueda.addEventListener("input", function () {
        const texto = this.value.trim().toLowerCase();
        medicamentoSeleccionado = null;
        medicamentoResultados.innerHTML = "";
        limpiarValidacion(medicamentoError);

        if (texto.length < 2) return;

        filtrarMedicamentos(texto).forEach(medicamento => {
            const item = document.createElement("button");
            item.type = "button";
            item.className = "list-group-item list-group-item-action";
            item.innerHTML = `<strong>${medicamento.nombre}</strong> - ${medicamento.presentacion}<br><span class="text-muted small">Stock: ${medicamento.stock} | Bs ${Number(medicamento.precio || 0).toFixed(2)}</span>`;
            item.addEventListener("click", () => seleccionarMedicamento(medicamento));
            medicamentoResultados.appendChild(item);
        });
    });

    function reindexar() {
        tabla.querySelectorAll("tr").forEach((fila, index) => {
            fila.querySelector(".id-input").name = `Venta.Detalles[${index}].IdMedicamento`;
            fila.querySelector(".cantidad-input").name = `Venta.Detalles[${index}].Cantidad`;
            fila.querySelector(".precio-input").name = `Venta.Detalles[${index}].PrecioUnitario`;
        });
    }

    function actualizarDetalleVacio() {
        const cantidadFilas = tabla.querySelectorAll("tr").length;
        detalleVacio.classList.toggle("d-none", cantidadFilas > 0);
    }

    function recalcular() {
        let total = 0;

        tabla.querySelectorAll("tr").forEach(fila => {
            const cantidad = Number(fila.querySelector(".cantidad-input").value || 0);
            const precio = Number(fila.dataset.precio || 0);
            const subtotal = cantidad * precio;
            fila.querySelector(".subtotal-cell").textContent = subtotal.toFixed(2);
            total += subtotal;
        });

        totalVenta.textContent = total.toFixed(2);
        totalConfirmacion.textContent = total.toFixed(2);
        actualizarDetalleVacio();
    }

    function agregarMedicamento() {
        if (!medicamentoSeleccionado) {
            mostrarValidacion(medicamentoError, "Debe seleccionar un medicamento.", medicamentoBusqueda);
            return;
        }

        const cantidad = 1;
        limpiarValidacion(medicamentoError);
        limpiarValidacion(detalleError);

        const filaExistente = Array.from(tabla.querySelectorAll("tr"))
            .find(fila => Number(fila.dataset.idMedicamento) === Number(medicamentoSeleccionado.id));

        if (filaExistente) {
            const cantidadInput = filaExistente.querySelector(".cantidad-input");
            cantidadInput.value = Number(cantidadInput.value || 0) + cantidad;
            if (!validarCantidadFila(filaExistente)) {
                validarCantidadesDetalle(true, true);
            }
            recalcular();
            limpiarSeleccionMedicamento();
            return;
        }

        const fila = document.createElement("tr");
        fila.dataset.idMedicamento = medicamentoSeleccionado.id;
        fila.dataset.precio = Number(medicamentoSeleccionado.precio || 0);
        fila.dataset.stock = Number(medicamentoSeleccionado.stock || 0);

        fila.innerHTML = `
            <td class="fw-semibold">
                ${medicamentoSeleccionado.nombre}<br>
                <span class="text-muted small">${medicamentoSeleccionado.presentacion}</span>
                <input type="hidden" class="id-input" value="${medicamentoSeleccionado.id}">
            </td>
            <td>${medicamentoSeleccionado.stock}</td>
            <td>
                <input type="number" min="1" class="form-control cantidad-input quantity-cell" value="${cantidad}">
            </td>
            <td>
                ${Number(medicamentoSeleccionado.precio || 0).toFixed(2)}
                <input type="hidden" class="precio-input" value="${Number(medicamentoSeleccionado.precio || 0).toFixed(2)}">
            </td>
            <td class="subtotal-cell fw-semibold">0.00</td>
            <td>
                <button type="button" class="btn btn-outline-danger btn-sm quitar-detalle">Quitar</button>
            </td>
        `;

        tabla.appendChild(fila);
        reindexar();
        if (!validarCantidadFila(fila)) {
            validarCantidadesDetalle(true, true);
        }
        recalcular();
        limpiarSeleccionMedicamento();
    }

    function limpiarSeleccionMedicamento() {
        medicamentoSeleccionado = null;
        medicamentoBusqueda.value = "";
        medicamentoResultados.innerHTML = "";
        limpiarErrorGeneral();
    }

    function obtenerMensajeCantidadFila(fila) {
        const cantidadInput = fila.querySelector(".cantidad-input");
        const cantidad = Number(cantidadInput.value || 0);
        const stock = Number(fila.dataset.stock || 0);

        if (cantidad <= 0) {
            return "La cantidad debe ser mayor que cero.";
        }

        if (cantidad > stock) {
            return "Una o mas cantidades superan el stock disponible.";
        }

        return "";
    }

    function validarCantidadFila(fila) {
        const cantidadInput = fila.querySelector(".cantidad-input");
        const mensaje = obtenerMensajeCantidadFila(fila);

        if (mensaje) {
            cantidadInput.classList.add("is-invalid");
            return false;
        }

        cantidadInput.classList.remove("is-invalid");
        return true;
    }

    function validarCantidadesDetalle(mostrarMensaje, enfocar) {
        for (const fila of tabla.querySelectorAll("tr")) {
            const mensaje = obtenerMensajeCantidadFila(fila);
            const cantidadInput = fila.querySelector(".cantidad-input");
            if (mensaje) {
                validarCantidadFila(fila);
                if (mostrarMensaje) {
                    mostrarValidacion(detalleError, mensaje, enfocar ? cantidadInput : null);
                }
                return false;
            }

            validarCantidadFila(fila);
        }

        limpiarValidacion(detalleError);
        return true;
    }

    function validarVenta() {
        limpiarErrorGeneral();

        if (!clienteId.value || Number(clienteId.value) <= 0 || !clienteActual) {
            const focoCliente = clienteBusquedaWrap.classList.contains("d-none")
                ? btnConsumidorFinal
                : clienteBusqueda;
            mostrarValidacion(clienteError, "Debe seleccionar un cliente valido.", focoCliente);
            return false;
        }

        if (!metodoPagoInput.value) {
            mostrarValidacion(pagoError, "Debe seleccionar un metodo de pago.", metodoPagoInput);
            return false;
        }

        if (tabla.querySelectorAll("tr").length === 0) {
            mostrarValidacion(detalleError, "Debe agregar al menos un medicamento.", medicamentoBusqueda);
            return false;
        }

        if (!validarCantidadesDetalle(true, true)) {
            return false;
        }

        limpiarValidaciones();
        return true;
    }

    agregarDetalle.addEventListener("click", agregarMedicamento);

    tabla.addEventListener("input", function (event) {
        if (event.target.classList.contains("cantidad-input")) {
            validarCantidadFila(event.target.closest("tr"));
            validarCantidadesDetalle(true, false);
            recalcular();
        }
    });

    tabla.addEventListener("click", function (event) {
        if (!event.target.classList.contains("quitar-detalle")) return;

        event.target.closest("tr").remove();
        reindexar();
        validarCantidadesDetalle(true, false);
        recalcular();
    });

    btnPrepararVenta.addEventListener("click", function () {
        if (!validarVenta()) return;

        modalConfirmarVenta.show();
    });

    document.addEventListener("click", function (event) {
        if (!event.target.closest(".position-relative")) {
            clienteResultados.innerHTML = "";
            medicamentoResultados.innerHTML = "";
        }
    });

    activarModoConsumidorFinal();
    actualizarDetalleVacio();
});
