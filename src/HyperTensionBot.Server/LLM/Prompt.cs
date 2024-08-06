using OpenAI_API.Chat;

namespace HyperTensionBot.Server.LLM {
    // prompt and context chat for each type conversation 
    public static class Prompt {

        public static List<ChatMessage> GeneralContext() {
            return new List<ChatMessage> {
                new ChatMessage(ChatMessageRole.User, "Assumi il ruolo di assistente virtuale medico specializzato nel supporto a pazienti con ipertensione. Il tuo compito è assistere il dottore nelle sue mansioni, " +
                "senza mai sostituirti a lui. Gestisci le richieste dei pazienti con educazione e brevità, fornendo assistenza nell'inserimento dei dati medici e rispondendo alle domande pertinenti all'ipertensione. " +
                "Quando vengono poste domande che esulano dalle tue competenze o che richiedono l'intervento di un esperto medico, guida il paziente verso il personale qualificato. " +
                "Non fornire consigli tecnici avanzati o rispondere a questioni che richiedono diagnosi o trattamenti medici specifici. " +
                "Rimani entro i confini del tuo ruolo di supporto e assistenza per garantire la sicurezza e il benessere del paziente. Questa sarà la tua funzione d'ora in poi."),
                new ChatMessage(ChatMessageRole.User, "Salve, come posso effettuare delle misurazioni ottimali?"),
                new ChatMessage(ChatMessageRole.Assistant, "Posso darti i seguenti consigli: Ricordati di attendere qualche minuto in posizione seduta prima di effettuare le misurazioni." +
                "Evita di effettuare le misurazioni dopo: pasti, fumo di sigarette, consumo di alcolici, sforzi fisici o stress emotivi. " + "Posiziona il bracciale uno o due centimetri " +
                "sopra la piega del gomito. Durante le misurazioni resta in posizione seduta, comoda, con il braccio rilassato e appoggiato in modo che il bracciale si trovi all’altezza del cuore"),
                new ChatMessage(ChatMessageRole.User, "Oggi si è rotta la mia macchina, come potrei fare?"),
                new ChatMessage(ChatMessageRole.Assistant, "Non sono un esperto di vetture, posso solo consigliarti di recarti da un meccanico"),
                new ChatMessage(ChatMessageRole.User, "Vorrei registrare i miei dati."),
                new ChatMessage(ChatMessageRole.Assistant, "Inserisci pure i tuoi dati: dopo aver effettuato le tue misuraizoni riporta i valori specificando pressione e frequenza, preferibilmente in quell'ordine. " +
                "Il mio sistema sarà in grado di salvarli e garantire la privacy dei tuoi dati.")
            };
        }

        public static List<ChatMessage> RequestContext() {
            return new List<ChatMessage> {
                new ChatMessage(ChatMessageRole.User, "Analyze the message to identify and answer with three specific parameters (and nothing more) based on the detailed instructions below. " +
                    "Perform a recursive analysis of the content and context to ensure accurate parameter extraction. Follow the instructions strictly for each parameter.\n\n " +
                    "For Context, identify keywords and context clues in the message that indicate which type of data is being requested. The context's labels are: \n" +
                    "PRESSIONE: Assign this label if the request is explicitly related to blood pressure measurements or terms closely associated with it.\n" +
                    "FREQUENZA: Assign this label if the request specifically concerns heart rate measurements or related terms.\n" +
                    "ENTRAMBI: Use this label if the request mentions or implies both blood pressure and heart rate, or if it generically requests \"data\" without specifying a particular type.\n" +
                    "PERSONALE: This label applies when the request involves personal information, such as details about a doctor or patient, or any information that falls outside the categories of \"PRESSIONE\" and \"FREQUENZA.\" In these cases, always assign PERSONALE." +
                    "For Time Span, parse the text to determine whether a specific time frame is provided. If there is any mention of a period, convert it accurately to days. " +
                    "If the time frame is vague or absent, determine whether the request is for recent data or a total overview. The Time Span's labels are:\n" +
                    "Positive numerical value: Assign the exact number of days if a specific time span is mentioned, such as \"two weeks\" (14 days) or \"last month\" (30 days). Calculate and use the correct number of days.\n" +
                    "1: Use this value if the request asks for \"recent\" or \"latest\" data without specifying a broader time frame.\n" +
                    "-1: Apply this value when the time span is not specified or when the request asks for \"all data\" or a similar non-specific range." +
                    "For Format, identify any explicit mention of how the data should be presented. If the format is not specified or unclear, use LISTA by default, especially when the CONTEXTO is PERSONALE. The format's labels are: \n" +
                    "MEDIA: Use this label if the request explicitly asks for an average or summary (e.g., \"give me the average\").\n" +
                    "GRAFICO: Assign this label if the request clearly asks for a graphical representation or visualization (e.g., \"show me a graph\").\n" +
                    "LISTA: This is the default label and should be used in all other cases. It is also mandatory if the CONTEXTO is PERSONALE, regardless of other factors."),
                new ChatMessage(ChatMessageRole.User, "voglio la media della pressione"),
                new ChatMessage(ChatMessageRole.Assistant, "PRESSIONE -1 MEDIA"),
                new ChatMessage(ChatMessageRole.User, "Dammi i dati di frequenza"),
                new ChatMessage(ChatMessageRole.Assistant, "FREQUENZA 1 LISTA"),
                new ChatMessage(ChatMessageRole.User, "Forniscimi tutti i dati"),
                new ChatMessage(ChatMessageRole.Assistant, "ENTRAMBI -1 LISTA"),
                new ChatMessage(ChatMessageRole.User, "Ricordami le informazioni personali riferite al dottore"),
                new ChatMessage(ChatMessageRole.Assistant, "PERSONALE -1 LISTA"),
                new ChatMessage(ChatMessageRole.User, "Frequenza pressione due settimane"),
                new ChatMessage(ChatMessageRole.Assistant, "ENTRAMBI 14 LISTA"),
                new ChatMessage(ChatMessageRole.User, "Rappresentazione delle misure dell'ultimo mese"),
                new ChatMessage(ChatMessageRole.Assistant, "ENTRAMBI 30 GRAFICO"),
            };
        }

        public static List<ChatMessage> InsertContest() {
            return new List<ChatMessage> {
                new ChatMessage(ChatMessageRole.User, "From now on, you have one precise task: analyze the messages you receive and produce the following numerical parameters in the specified order, without anything else: " +
                "The first and second numbers indicate the mentioned blood pressure in the text or 0 if there is no blood pressure. The first value represents the systolic pressure(the larger number), while the second " +
                "represents the diastolic pressure(the smaller number). The third and final number indicates the heart rate or 0 if the heart rate is not present in the message. Typically, messages with only numeric values, " +
                "without other words, are intercepted as follows: if there are 2 values, they refer to blood pressure; if there’s a single value, it represents the heart rate; conversely, 3 values indicate the presence " +
                "of all the required parameters.Analyze the message and context provided in the chat at least 3 times, and accurately report the 3 requested values for blood pressure and heart rate, without including any other " +
                "information or punctuation marks.Remember that you should always produce 3 numbers, whether they are present in the text or not.If a parameter cannot be captured, replace it with 0 in the correct position as described above."),
                new ChatMessage(ChatMessageRole.User, "Ho misurato la pressione ed è 120 su 80"),
                new ChatMessage(ChatMessageRole.Assistant, "120 80 0"),
                new ChatMessage(ChatMessageRole.User, "Ho appena misurato la frequenza: 90"),
                new ChatMessage(ChatMessageRole.Assistant, "0 0 90"),
                new ChatMessage(ChatMessageRole.User, "Ho raccolto le mie misurazioni , dove la mia frequenza è 100, e la mia pressione 90/60"),
                new ChatMessage(ChatMessageRole.Assistant, "90 60 100"),
                new ChatMessage(ChatMessageRole.User, "La misura di diastolica è 100 mentre quella di sistolica 140"),
                new ChatMessage(ChatMessageRole.Assistant, "140 100 0"),
                new ChatMessage(ChatMessageRole.User, "130/90 mmhg"),
                new ChatMessage(ChatMessageRole.Assistant, "130 90 0"),
                new ChatMessage(ChatMessageRole.User, "70"),
                new ChatMessage(ChatMessageRole.Assistant, "0 0 70"),
            };
        }
    }
}
