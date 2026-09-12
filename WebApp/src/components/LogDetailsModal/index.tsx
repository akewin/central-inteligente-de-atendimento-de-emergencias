import { Modal, Box, Typography, Paper, IconButton } from "@mui/material";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faClose, faEye } from "@fortawesome/free-solid-svg-icons";
import { useEffect, useState } from "react";

import type { SystemLogRead } from "../../interfaces";
import { useReports } from "../../hooks/useReports";
import { useNotification } from "../../hooks";
import { getErrorMessage } from "../../helpers";
import JsonWrapper from "../JsonWrapper";

interface Props {
  open: boolean;
  logId: number | null;
  onClose: () => void;
}

export default function LogDetailsModal({ open, logId, onClose }: Props) {
  const { fetchLogDetails } = useReports();
  const { showNotification } = useNotification();

  const [log, setLog] = useState<SystemLogRead | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!logId) {
      setLog(null);
      return;
    }

    const run = async () => {
      try {
        setLoading(true);
        const data = await fetchLogDetails(logId);
        setLog(data);
      } catch (err) {
        showNotification(getErrorMessage(err), "error");
      } finally {
        setLoading(false);
      }
    };

    run();
  }, [logId, fetchLogDetails, showNotification]);

  if (!open) return null;

  return (
    <Modal open={open} onClose={onClose}>
      <Box
        sx={{
          position: "absolute",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          width: { xs: "90%", sm: 600, md: 900 },
          maxHeight: "90vh",
          overflow: "auto",
        }}
      >
        <Paper sx={{ p: 3 }}>
          <Box
            display="flex"
            justifyContent="space-between"
            alignItems="center"
            mb={2}
          >
            <Typography variant="h6">
              <FontAwesomeIcon icon={faEye} style={{ marginRight: 8 }} />
              Detalhes do Log {logId}
            </Typography>
            <IconButton onClick={onClose}>
              <FontAwesomeIcon icon={faClose} />
            </IconButton>
          </Box>

          {loading || !log ? (
            <Typography>Carregando...</Typography>
          ) : (
            <>
              <Box display="flex" justifyContent="space-between" mb={3} gap={2}>
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Gerado por
                  </Typography>
                  <Typography variant="body1">{log.generatedBy}</Typography>
                </Box>

                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Ação
                  </Typography>
                  <Typography variant="body1">{log.action}</Typography>
                </Box>

                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Data/Hora
                  </Typography>
                  <Typography variant="body1">
                    {new Date(log.createdAt).toLocaleString()}
                  </Typography>
                </Box>
              </Box>

              {log.data && (
                <Box display="flex" gap={3} width="100%">
                  {log.data.type === "create" && (
                    <JsonWrapper
                      title="Payload utilizado"
                      jsonContent={log.data.created}
                    />
                  )}

                  {log.data.type === "update" && (
                    <>
                      <JsonWrapper
                        title="Estado anterior"
                        jsonContent={log.data.prevState}
                      />
                      <JsonWrapper
                        title="Estado após atualização"
                        jsonContent={log.data.currState}
                      />
                    </>
                  )}
                </Box>
              )}
            </>
          )}
        </Paper>
      </Box>
    </Modal>
  );
}
